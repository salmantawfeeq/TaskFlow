using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// An organizational department (e.g. "Engineering", "Marketing").
/// Users belong to at most one Department; a Department can contain
/// multiple Teams.
/// </summary>
public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // ---- Navigation properties ----

    public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
