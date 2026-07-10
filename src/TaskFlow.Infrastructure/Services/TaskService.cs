using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskItemEntity = TaskFlow.Domain.Entities.TaskItem;

namespace TaskFlow.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public TaskService(IUnitOfWork uow, IMapper mapper, INotificationService notificationService)
    {
        _uow = uow;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<TaskItemDto>> GetFilteredAsync(TaskFilterDto filter)
    {
        var query = _uow.Tasks.Query()
            .Include(t => t.Project)
            .Include(t => t.Assignments).ThenInclude(a => a.User)
            .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .AsQueryable();

        if (filter.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == filter.ProjectId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        if (!string.IsNullOrEmpty(filter.AssigneeUserId))
        {
            query = query.Where(t => t.Assignments.Any(a => a.UserId == filter.AssigneeUserId));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(term));
        }

        if (filter.OnlyOverdue)
        {
            var now = DateTime.UtcNow;
            query = query.Where(t => t.DueDate != null && t.DueDate < now && t.Status != Domain.Enums.TaskStatus.Done);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<TaskItemDto>
        {
            Items = _mapper.Map<List<TaskItemDto>>(items),
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaskDetailDto?> GetDetailAsync(int id)
    {
        var task = await _uow.Tasks.GetWithDetailsAsync(id);
        return task == null ? null : _mapper.Map<TaskDetailDto>(task);
    }

    public async Task<IReadOnlyList<TaskItemDto>> GetKanbanBoardAsync(int projectId)
    {
        var tasks = await _uow.Tasks.GetKanbanTasksAsync(projectId);
        return _mapper.Map<List<TaskItemDto>>(tasks);
    }

    public async Task<IReadOnlyList<TaskItemDto>> GetCalendarTasksAsync(int? projectId, DateTime rangeStart, DateTime rangeEnd)
    {
        var query = _uow.Tasks.Query()
            .Include(t => t.Project)
            .Include(t => t.Assignments).ThenInclude(a => a.User)
            .Where(t => t.DueDate != null && t.DueDate >= rangeStart && t.DueDate <= rangeEnd);

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        var tasks = await query.ToListAsync();
        return _mapper.Map<List<TaskItemDto>>(tasks);
    }

    public async Task<ServiceResult<int>> CreateAsync(CreateTaskDto dto)
    {
        var maxOrder = await _uow.Tasks.Query()
            .Where(t => t.ProjectId == dto.ProjectId && t.Status == Domain.Enums.TaskStatus.ToDo)
            .Select(t => (int?)t.BoardOrder)
            .MaxAsync() ?? 0;

        var task = new TaskItemEntity
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            StartDate = dto.StartDate,
            ProjectId = dto.ProjectId,
            ParentTaskId = dto.ParentTaskId,
            CreatedByUserId = dto.CreatedByUserId,
            Status = Domain.Enums.TaskStatus.ToDo,
            BoardOrder = maxOrder + 1
        };

        foreach (var userId in dto.AssigneeUserIds.Distinct())
        {
            task.Assignments.Add(new TaskAssignment { UserId = userId, AssignedByUserId = dto.CreatedByUserId });
        }

        foreach (var labelId in dto.LabelIds.Distinct())
        {
            task.TaskLabels.Add(new TaskLabel { LabelId = labelId });
        }

        await _uow.Tasks.AddAsync(task);
        await _uow.SaveChangesAsync();

        await LogActivityAsync(task.Id, dto.CreatedByUserId, ActivityActionType.Created, $"created the task \"{task.Title}\".");

        foreach (var userId in dto.AssigneeUserIds.Distinct())
        {
            await _notificationService.NotifyAsync(
                userId,
                NotificationType.TaskAssigned,
                "New task assigned",
                $"You were assigned to \"{task.Title}\".",
                $"/Tasks/Details/{task.Id}",
                relatedTaskId: task.Id);
        }

        await _uow.SaveChangesAsync();

        return ServiceResult<int>.Success(task.Id);
    }

    public async Task<ServiceResult> UpdateAsync(UpdateTaskDto dto)
    {
        var task = await _uow.Tasks.GetByIdAsync(dto.Id);
        if (task == null)
        {
            return ServiceResult.Failure("Task not found.");
        }

        var priorityChanged = task.Priority != dto.Priority;
        var oldPriority = task.Priority;

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.StartDate = dto.StartDate;
        task.UpdatedAt = DateTime.UtcNow;

        _uow.Tasks.Update(task);

        if (priorityChanged)
        {
            await LogActivityAsync(task.Id, task.CreatedByUserId, ActivityActionType.PriorityChanged,
                $"changed priority from {oldPriority} to {dto.Priority}.");
        }

        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, string deletedByUserId)
    {
        var task = await _uow.Tasks.GetByIdAsync(id);
        if (task == null)
        {
            return ServiceResult.Failure("Task not found.");
        }

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;

        _uow.Tasks.Update(task);
        await LogActivityAsync(task.Id, deletedByUserId, ActivityActionType.Deleted, "deleted this task.");
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> MoveTaskAsync(MoveTaskDto dto)
    {
        var task = await _uow.Tasks.GetByIdAsync(dto.TaskId);
        if (task == null)
        {
            return ServiceResult.Failure("Task not found.");
        }

        var oldStatus = task.Status;
        task.Status = dto.NewStatus;
        task.BoardOrder = dto.NewBoardOrder;
        task.UpdatedAt = DateTime.UtcNow;

        if (dto.NewStatus == Domain.Enums.TaskStatus.Done && oldStatus != Domain.Enums.TaskStatus.Done)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (dto.NewStatus != Domain.Enums.TaskStatus.Done)
        {
            task.CompletedAt = null;
        }

        _uow.Tasks.Update(task);

        if (oldStatus != dto.NewStatus)
        {
            await LogActivityAsync(
                task.Id,
                dto.ChangedByUserId,
                ActivityActionType.StatusChanged,
                $"moved this task from {oldStatus} to {dto.NewStatus}.");
        }

        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> AssignUserAsync(int taskId, string userId, string assignedByUserId)
    {
        var task = await _uow.Tasks.GetWithDetailsAsync(taskId);
        if (task == null)
        {
            return ServiceResult.Failure("Task not found.");
        }

        if (task.Assignments.Any(a => a.UserId == userId))
        {
            return ServiceResult.Failure("User is already assigned to this task.");
        }

        task.Assignments.Add(new TaskAssignment { UserId = userId, AssignedByUserId = assignedByUserId });
        await LogActivityAsync(taskId, assignedByUserId, ActivityActionType.Assigned, "assigned a new user to this task.");
        await _uow.SaveChangesAsync();

        await _notificationService.NotifyAsync(
            userId,
            NotificationType.TaskAssigned,
            "New task assigned",
            $"You were assigned to \"{task.Title}\".",
            $"/Tasks/Details/{taskId}",
            relatedTaskId: taskId);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UnassignUserAsync(int taskId, string userId)
    {
        var task = await _uow.Tasks.GetWithDetailsAsync(taskId);
        if (task == null)
        {
            return ServiceResult.Failure("Task not found.");
        }

        var assignment = task.Assignments.FirstOrDefault(a => a.UserId == userId);
        if (assignment == null)
        {
            return ServiceResult.Failure("User is not assigned to this task.");
        }

        task.Assignments.Remove(assignment);
        await LogActivityAsync(taskId, userId, ActivityActionType.Unassigned, "removed a user from this task.");
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> AddLabelAsync(int taskId, int labelId)
    {
        var task = await _uow.Tasks.GetWithDetailsAsync(taskId);
        if (task == null)
        {
            return ServiceResult.Failure("Task not found.");
        }

        if (task.TaskLabels.Any(tl => tl.LabelId == labelId))
        {
            return ServiceResult.Failure("Label already applied.");
        }

        task.TaskLabels.Add(new TaskLabel { LabelId = labelId, TaskItemId = taskId });
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveLabelAsync(int taskId, int labelId)
    {
        var task = await _uow.Tasks.GetWithDetailsAsync(taskId);
        if (task == null)
        {
            return ServiceResult.Failure("Task not found.");
        }

        var taskLabel = task.TaskLabels.FirstOrDefault(tl => tl.LabelId == labelId);
        if (taskLabel == null)
        {
            return ServiceResult.Failure("Label not found on this task.");
        }

        task.TaskLabels.Remove(taskLabel);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<int>> AddChecklistItemAsync(int taskId, string text)
    {
        var task = await _uow.Tasks.GetByIdAsync(taskId);
        if (task == null)
        {
            return ServiceResult<int>.Failure("Task not found.");
        }

        var maxOrder = await _uow.ChecklistItems.Query()
            .Where(c => c.TaskItemId == taskId)
            .Select(c => (int?)c.SortOrder)
            .MaxAsync() ?? 0;

        var item = new ChecklistItem { TaskItemId = taskId, Text = text, SortOrder = maxOrder + 1 };
        await _uow.ChecklistItems.AddAsync(item);
        await _uow.SaveChangesAsync();

        return ServiceResult<int>.Success(item.Id);
    }

    public async Task<ServiceResult> ToggleChecklistItemAsync(int checklistItemId, bool isCompleted)
    {
        var item = await _uow.ChecklistItems.GetByIdAsync(checklistItemId);
        if (item == null)
        {
            return ServiceResult.Failure("Checklist item not found.");
        }

        item.IsCompleted = isCompleted;
        item.CompletedAt = isCompleted ? DateTime.UtcNow : null;

        _uow.ChecklistItems.Update(item);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveChecklistItemAsync(int checklistItemId)
    {
        var item = await _uow.ChecklistItems.GetByIdAsync(checklistItemId);
        if (item == null)
        {
            return ServiceResult.Failure("Checklist item not found.");
        }

        _uow.ChecklistItems.Remove(item);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<int>> AddCommentAsync(int taskId, string userId, string content, int? parentCommentId)
    {
        var task = await _uow.Tasks.GetWithDetailsAsync(taskId);
        if (task == null)
        {
            return ServiceResult<int>.Failure("Task not found.");
        }

        var comment = new Comment
        {
            TaskItemId = taskId,
            UserId = userId,
            Content = content,
            ParentCommentId = parentCommentId
        };

        await _uow.Comments.AddAsync(comment);
        await LogActivityAsync(taskId, userId, ActivityActionType.CommentAdded, "added a comment.");
        await _uow.SaveChangesAsync();

        // Notify all other assignees that a new comment was posted.
        foreach (var assignment in task.Assignments.Where(a => a.UserId != userId))
        {
            await _notificationService.NotifyAsync(
                assignment.UserId,
                NotificationType.CommentAdded,
                "New comment on your task",
                $"A new comment was added to \"{task.Title}\".",
                $"/Tasks/Details/{taskId}",
                relatedTaskId: taskId);
        }

        return ServiceResult<int>.Success(comment.Id);
    }

    public async Task<ServiceResult> DeleteCommentAsync(int commentId, string requestingUserId)
    {
        var comment = await _uow.Comments.GetByIdAsync(commentId);
        if (comment == null)
        {
            return ServiceResult.Failure("Comment not found.");
        }

        if (comment.UserId != requestingUserId)
        {
            return ServiceResult.Failure("You can only delete your own comments.");
        }

        _uow.Comments.Remove(comment);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    private async Task LogActivityAsync(int taskId, string userId, ActivityActionType actionType, string description)
    {
        await _uow.ActivityLogs.AddAsync(new ActivityLog
        {
            TaskItemId = taskId,
            RelatedTo = EntityRelationType.TaskItem,
            UserId = userId,
            ActionType = actionType,
            Description = description
        });
    }
}
