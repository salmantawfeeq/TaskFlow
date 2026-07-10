namespace TaskFlow.Domain.Entities;

/// <summary>
/// Join entity between TaskItem and Label. Kept as a plain composite-key
/// join table (no extra metadata needed beyond the relationship itself).
/// </summary>
public class TaskLabel
{
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public int LabelId { get; set; }
    public Label Label { get; set; } = null!;
}
