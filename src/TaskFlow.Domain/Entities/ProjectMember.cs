using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Explicit join entity between ApplicationUser and Project, tracking who is
/// a member of which project and when they were added.
/// </summary>
public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this member can manage the project (edit/delete/archive),
    /// as opposed to only working on tasks within it.
    /// </summary>
    public bool IsProjectManager { get; set; } = false;
}
