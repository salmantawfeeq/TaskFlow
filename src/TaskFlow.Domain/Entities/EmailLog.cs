using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

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
