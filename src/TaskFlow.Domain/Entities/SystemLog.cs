using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Application-level log entry (errors, warnings, significant system
/// events) surfaced in the Admin Panel's "Logs" screen. Distinct from
/// ActivityLog, which tracks user-facing business actions on
/// Projects/Tasks - this table is for technical/operational logging
/// (exceptions caught by global exception handling, failed logins, etc).
/// Serilog also writes structured logs to this table via its SQL Server sink.
/// </summary>
public class SystemLog : BaseEntity
{
    /// <summary>
    /// Log severity: "Information", "Warning", "Error", "Critical".
    /// </summary>
    public string Level { get; set; } = "Information";

    public string Message { get; set; } = string.Empty;

    public string? ExceptionDetails { get; set; }

    public string? Source { get; set; }

    public string? UserId { get; set; }
}
