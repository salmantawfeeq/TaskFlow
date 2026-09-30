using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public ActivityActionType ActionType { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional JSON snapshot of the old/new values for fields that changed,
    /// useful for detailed audit views without needing extra columns per field.
    /// </summary>
    public string? MetadataJson { get; set; }

    public EntityRelationType RelatedTo { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
