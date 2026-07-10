using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Join entity between TaskItem and ApplicationUser, tracking which users
/// are assigned to a task and when. Supports multiple assignees per task.
/// </summary>
public class TaskAssignment : BaseEntity
{
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The user who performed the assignment (for audit purposes) - may
    /// differ from the assignee, e.g. a manager assigning to an employee.
    /// </summary>
    public string? AssignedByUserId { get; set; }
}
