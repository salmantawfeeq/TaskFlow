namespace TaskFlow.Domain.Enums;

/// <summary>
/// Distinguishes the kind of event a Notification represents, so the UI can
/// pick an appropriate icon/color and the user can filter their notification feed.
/// </summary>
public enum NotificationType
{
    TaskAssigned = 1,
    TaskDueSoon = 2,
    TaskOverdue = 3,
    TaskCompleted = 4,
    CommentAdded = 5,
    MentionedInComment = 6,
    ProjectInvite = 7,
    TeamInvite = 8,
    StatusChanged = 9,
    GeneralAnnouncement = 10
}
