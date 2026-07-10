using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A colored label/tag that can be attached to TaskItems (e.g. "Bug",
/// "Feature", "Urgent"). Similar to Trello labels. Scoped per-project so
/// each project can define its own labeling scheme.
/// </summary>
public class Label : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string ColorHex { get; set; } = "#10B981";

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // ---- Navigation properties ----

    public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
}
