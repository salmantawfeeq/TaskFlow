using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Interfaces.Services;

public interface ITaskService
{
    Task<PagedResult<TaskItemDto>> GetFilteredAsync(TaskFilterDto filter);

    Task<TaskDetailDto?> GetDetailAsync(int id);

    /// <summary>
    /// Returns all tasks for a project, organized for direct binding to a
    /// Kanban board (grouped by status on the client or server side).
    /// </summary>
    Task<IReadOnlyList<TaskItemDto>> GetKanbanBoardAsync(int projectId);

    /// <summary>
    /// Tasks with a DueDate, for the Calendar View.
    /// </summary>
    Task<IReadOnlyList<TaskItemDto>> GetCalendarTasksAsync(int? projectId, DateTime rangeStart, DateTime rangeEnd);

    Task<ServiceResult<int>> CreateAsync(CreateTaskDto dto);

    Task<ServiceResult> UpdateAsync(UpdateTaskDto dto);

    Task<ServiceResult> DeleteAsync(int id, string deletedByUserId);

    /// <summary>
    /// Handles drag-and-drop: updates Status + BoardOrder and writes an
    /// ActivityLog entry describing the status transition.
    /// </summary>
    Task<ServiceResult> MoveTaskAsync(MoveTaskDto dto);

    Task<ServiceResult> AssignUserAsync(int taskId, string userId, string assignedByUserId);

    Task<ServiceResult> UnassignUserAsync(int taskId, string userId);

    Task<ServiceResult> AddLabelAsync(int taskId, int labelId);

    Task<ServiceResult> RemoveLabelAsync(int taskId, int labelId);

    Task<ServiceResult<int>> AddChecklistItemAsync(int taskId, string text);

    Task<ServiceResult> ToggleChecklistItemAsync(int checklistItemId, bool isCompleted);

    Task<ServiceResult> RemoveChecklistItemAsync(int checklistItemId);

    Task<ServiceResult<int>> AddCommentAsync(int taskId, string userId, string content, int? parentCommentId);

    Task<ServiceResult> DeleteCommentAsync(int commentId, string requestingUserId);
}
