namespace TaskFlow.Domain.Enums;

/// <summary>
/// Lifecycle status of a Project.
/// </summary>
public enum ProjectStatus
{
    Planning = 1,
    Active = 2,
    OnHold = 3,
    Completed = 4,
    Archived = 5
}
