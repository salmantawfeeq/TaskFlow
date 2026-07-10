using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Explicit join entity between ApplicationUser and Team (rather than an
/// implicit EF many-to-many) because we need to store extra data about the
/// membership itself: when they joined and whether they're a lead.
/// </summary>
public class TeamMember : BaseEntity
{
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsTeamLead { get; set; } = false;
}
