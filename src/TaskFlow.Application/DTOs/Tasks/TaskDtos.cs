using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Tasks;

public class TaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Domain.Enums.TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public int BoardOrder { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectColorHex { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public int ChecklistProgressPercentage { get; set; }
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
    public List<TaskAssigneeDto> Assignees { get; set; } = new();
    public List<TaskLabelDto> Labels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class TaskDetailDto : TaskItemDto
{
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public int? ParentTaskId { get; set; }
    public List<ChecklistItemDto> ChecklistItems { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();
    public List<AttachmentDto> Attachments { get; set; } = new();
    public List<ActivityLogDto> Activities { get; set; } = new();
}

public class TaskAssigneeDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
}

public class TaskLabelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
}

public class ChecklistItemDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string? UserProfileImagePath { get; set; }
    public int? ParentCommentId { get; set; }
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ActivityLogDto
{
    public int Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string? UserProfileImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public int ProjectId { get; set; }
    public int? ParentTaskId { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public List<string> AssigneeUserIds { get; set; } = new();
    public List<int> LabelIds { get; set; } = new();
}

public class UpdateTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
}

public class MoveTaskDto
{
    public int TaskId { get; set; }
    public Domain.Enums.TaskStatus NewStatus { get; set; }
    public int NewBoardOrder { get; set; }
    public string ChangedByUserId { get; set; } = string.Empty;
}

public class TaskFilterDto
{
    public int? ProjectId { get; set; }
    public Domain.Enums.TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public string? AssigneeUserId { get; set; }
    public string? SearchTerm { get; set; }
    public bool OnlyOverdue { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
