using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class UserInvite : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Secure random token embedded in the invite link/email.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public string InvitedByUserId { get; set; } = string.Empty;
    public ApplicationUser InvitedByUser { get; set; } = null!;

    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    public bool IsAccepted { get; set; } = false;

    public DateTime? AcceptedAt { get; set; }
}
