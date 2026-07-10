namespace TaskFlow.Domain.Enums;

/// <summary>
/// Status of a TaskItem. These values map directly to the columns on the
/// Kanban board (ToDo -> InProgress -> InReview -> Done), plus a Blocked
/// state for tasks that are stuck waiting on something external.
/// </summary>
public enum TaskStatus
{
    ToDo = 1,
    InProgress = 2,
    InReview = 3,
    Done = 4,
    Blocked = 5
}
