using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A single checklist line item within a TaskItem (e.g. "Write unit tests",
/// "Update documentation"). Simpler than a full sub-task - just a
/// checkbox with text, used for granular progress tracking within one task.
/// </summary>
public class ChecklistItem : BaseEntity
{
    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Display order within the checklist.
    /// </summary>
    public int SortOrder { get; set; } = 0;

    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
}
