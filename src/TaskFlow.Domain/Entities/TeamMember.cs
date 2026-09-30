using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class TeamMember : BaseEntity
{
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsTeamLead { get; set; } = false;
}
