using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // ---- Navigation properties ----

    public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
