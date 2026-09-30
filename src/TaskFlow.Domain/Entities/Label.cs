using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Label : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string ColorHex { get; set; } = "#10B981";

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // ---- Navigation properties ----

    public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
}
