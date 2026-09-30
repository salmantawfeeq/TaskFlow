using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

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
