using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A file uploaded and attached to either a Project or a TaskItem.
/// Modeled with two nullable FKs (rather than true polymorphism) to keep
/// the relational schema simple and enforce referential integrity via
/// normal foreign keys; exactly one of ProjectId/TaskItemId should be set
/// (enforced in the Application layer's validation, not the database).
/// </summary>
public class Attachment : BaseEntity
{
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Relative path under wwwroot/uploads where the physical file is stored.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public EntityRelationType RelatedTo { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public string UploadedByUserId { get; set; } = string.Empty;
    public ApplicationUser UploadedByUser { get; set; } = null!;
}
