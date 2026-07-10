namespace TaskFlow.Domain.Enums;

/// <summary>
/// Priority level of a TaskItem. Ordered numerically so it can be sorted
/// and compared directly (e.g. "show tasks with priority >= High").
/// </summary>
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
