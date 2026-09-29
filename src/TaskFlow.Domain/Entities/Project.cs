using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class Project : AuditableSoftDeleteEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Hex color used for the project's card/badge on dashboards and lists.
    /// </summary>
    public string ColorHex { get; set; } = "#3B82F6";

    public bool IsArchived { get; set; } = false;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>
    /// The user who created and owns overall responsibility for the project.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;

    /// <summary>
    /// Optional: the team primarily responsible for this project. Nullable
    /// because a project can be created ad-hoc before a team is assigned.
    /// </summary>
    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    // ---- Navigation properties ----

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    public ICollection<ActivityLog> Activities { get; set; } = new List<ActivityLog>();

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    /// <summary>
    /// Computed progress percentage (0-100) based on completed vs total tasks.
    /// Not mapped to the database - calculated on read from the Tasks collection.
    /// </summary>
    public int ProgressPercentage =>
        Tasks.Count == 0
            ? 0
            : (int)Math.Round(Tasks.Count(t => t.Status == Enums.TaskStatus.Done) * 100.0 / Tasks.Count);
}
