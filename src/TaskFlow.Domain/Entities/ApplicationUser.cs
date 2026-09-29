using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Relative path (under wwwroot) to the user's profile picture, or null
    /// if they are using the default avatar.
    /// </summary>
    public string? ProfileImagePath { get; set; }

    public string? JobTitle { get; set; }

    public string? Bio { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// FK to the Department this user belongs to. Nullable because a brand
    /// new user may not be assigned to a department yet.
    /// </summary>
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    // ---- Navigation properties ----

    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();

    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();

    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();

    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();

    public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public ICollection<ActivityLog> Activities { get; set; } = new List<ActivityLog>();

    /// <summary>
    /// Convenience read-only property combining first/last name for display.
    /// Not mapped to a database column (computed in the EF configuration as [NotMapped]).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}
