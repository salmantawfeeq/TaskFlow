namespace TaskFlow.Domain.Enums;

/// <summary>
/// Identifies whether a polymorphic-ish record (ActivityLog, Attachment)
/// relates to a Project or a TaskItem. Kept simple (no true polymorphism)
/// to stay friendly to relational modeling and EF Core.
/// </summary>
public enum EntityRelationType
{
    Project = 1,
    TaskItem = 2
}
