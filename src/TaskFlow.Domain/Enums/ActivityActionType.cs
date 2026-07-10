namespace TaskFlow.Domain.Enums;

/// <summary>
/// Type of action recorded in an ActivityLog entry. Used to build the
/// "Activity History" timeline shown on task and project detail pages.
/// </summary>
public enum ActivityActionType
{
    Created = 1,
    Updated = 2,
    StatusChanged = 3,
    Assigned = 4,
    Unassigned = 5,
    CommentAdded = 6,
    AttachmentAdded = 7,
    AttachmentRemoved = 8,
    PriorityChanged = 9,
    DueDateChanged = 10,
    ChecklistItemAdded = 11,
    ChecklistItemCompleted = 12,
    LabelAdded = 13,
    LabelRemoved = 14,
    Archived = 15,
    Restored = 16,
    Deleted = 17
}
