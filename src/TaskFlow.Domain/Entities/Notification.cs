using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional relative URL the notification should navigate to when clicked
    /// (e.g. link to the task detail page).
    /// </summary>
    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Optional FK to the related task, for quick lookups without parsing LinkUrl.
    /// </summary>
    public int? RelatedTaskItemId { get; set; }
    public TaskItem? RelatedTaskItem { get; set; }

    public int? RelatedProjectId { get; set; }
    public Project? RelatedProject { get; set; }
}
