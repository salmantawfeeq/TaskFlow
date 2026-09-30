using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem : AuditableSoftDeleteEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.ToDo;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Position/order of this task within its Kanban column, used to persist
    /// manual drag-and-drop ordering (lower value = higher/earlier in the column).
    /// </summary>
    public int BoardOrder { get; set; } = 0;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Optional self-referencing FK to support subtasks (a TaskItem that is
    /// itself a checklist-style child of a larger parent task).
    /// </summary>
    public int? ParentTaskId { get; set; }
    public TaskItem? ParentTask { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = null!;

    // ---- Navigation properties ----

    public ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();

    public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();

    public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();

    public ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public ICollection<ActivityLog> Activities { get; set; } = new List<ActivityLog>();

    /// <summary>
    /// True if the task has a due date in the past and is not yet Done.
    /// Not mapped - computed at read time for UI badges ("Overdue").
    /// </summary>
    public bool IsOverdue =>
        DueDate.HasValue && DueDate.Value.Date < DateTime.UtcNow.Date && Status != Enums.TaskStatus.Done;

    /// <summary>
    /// Checklist completion percentage (0-100), used for progress bars on
    /// the task card. Not mapped - computed at read time.
    /// </summary>
    public int ChecklistProgressPercentage =>
        ChecklistItems.Count == 0
            ? 0
            : (int)Math.Round(ChecklistItems.Count(c => c.IsCompleted) * 100.0 / ChecklistItems.Count);
}
