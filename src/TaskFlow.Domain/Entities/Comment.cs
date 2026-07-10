using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A comment posted by a user on a TaskItem. Supports basic threading via
/// a self-referencing ParentCommentId for replies.
/// </summary>
public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Optional FK for reply-to-comment threading.
    /// </summary>
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }

    public bool IsEdited { get; set; } = false;

    // ---- Navigation properties ----

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
