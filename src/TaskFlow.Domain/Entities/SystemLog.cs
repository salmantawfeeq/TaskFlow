using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

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
