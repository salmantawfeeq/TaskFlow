using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A pending invitation sent to an email address to join a Team (and,
/// through it, its Department/Projects). The invited person may not yet
/// have an account; when they register using the invite token, they are
/// automatically added to the target Team.
/// </summary>
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
