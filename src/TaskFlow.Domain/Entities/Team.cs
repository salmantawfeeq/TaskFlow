using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A team is a working group of users, usually scoped within a Department,
/// that collaborates on one or more Projects.
/// </summary>
public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    /// <summary>
    /// The user who created/leads this team.
    /// </summary>
    public string TeamLeadId { get; set; } = string.Empty;
    public ApplicationUser TeamLead { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    // ---- Navigation properties ----

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
