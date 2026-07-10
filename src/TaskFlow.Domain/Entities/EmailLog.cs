using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Records a "sent" email without actually dispatching one over SMTP.
/// Since the spec calls for email simulation rather than a real mail
/// server, every call to IEmailService writes a row here instead of (or
/// in addition to, if SMTP is later configured) sending real mail. This
/// gives admins a way to inspect what the system "would have emailed"
/// via the Admin Panel's Logs screen.
/// </summary>
public class EmailLog : BaseEntity
{
    public string ToEmail { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Short machine-readable category, e.g. "EmailConfirmation",
    /// "PasswordReset", "TaskAssigned", "TaskDueSoon".
    /// </summary>
    public string EmailType { get; set; } = string.Empty;

    public bool IsSimulated { get; set; } = true;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
