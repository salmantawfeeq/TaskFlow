using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Hex color code (e.g. "#4F46E5") used to render category badges/chips
    /// consistently across the Projects list and Kanban board.
    /// </summary>
    public string ColorHex { get; set; } = "#6366F1";

    public bool IsActive { get; set; } = true;

    // ---- Navigation properties ----

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
