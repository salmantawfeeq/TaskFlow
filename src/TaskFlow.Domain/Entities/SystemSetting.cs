using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A single admin-configurable system setting, stored as a key/value pair
/// so new settings can be added from the Admin Panel without schema
/// migrations. Examples: "Site.Name", "Uploads.MaxFileSizeMb",
/// "Notifications.DueSoonThresholdHours".
/// </summary>
public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Logical group for display in the settings UI, e.g. "General",
    /// "Notifications", "Uploads", "Security".
    /// </summary>
    public string Category { get; set; } = "General";
}
