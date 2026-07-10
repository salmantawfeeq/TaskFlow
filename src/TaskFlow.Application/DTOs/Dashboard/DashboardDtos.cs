namespace TaskFlow.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TasksAssignedToMe { get; set; }
    public int TeamMembersCount { get; set; }
    public double OverallCompletionRate { get; set; }
}

public class TaskStatusChartDto
{
    public int ToDo { get; set; }
    public int InProgress { get; set; }
    public int InReview { get; set; }
    public int Done { get; set; }
    public int Blocked { get; set; }
}

public class TaskPriorityChartDto
{
    public int Low { get; set; }
    public int Medium { get; set; }
    public int High { get; set; }
    public int Critical { get; set; }
}

/// <summary>
/// One data point in a time-series chart (e.g. tasks completed per day
/// over the last 30 days), consumed directly by Chart.js on the client.
/// </summary>
public class ChartDataPointDto
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class ProjectProgressDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
}

public class RecentActivityDto
{
    public string Description { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string? UserProfileImagePath { get; set; }
    public string? LinkUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
