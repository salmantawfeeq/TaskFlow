namespace TaskFlow.Application.DTOs.Reports;

public class ReportFilterDto
{
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    public int? ProjectId { get; set; }
    public string? UserId { get; set; }
    public int? DepartmentId { get; set; }
}

public class CompletedTasksReportDto
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string CompletedByName { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public string Priority { get; set; } = string.Empty;
    public int DaysToComplete { get; set; }
}

public class ProductivityReportDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public int TasksCompleted { get; set; }
    public int TasksAssigned { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionDays { get; set; }
    public int OverdueTasks { get; set; }
}

public class EmployeePerformanceReportDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? DepartmentName { get; set; }
    public int TotalTasksAssigned { get; set; }
    public int TasksCompletedOnTime { get; set; }
    public int TasksCompletedLate { get; set; }
    public int TasksOverdueNow { get; set; }
    public double OnTimeCompletionRate { get; set; }
    public int CommentsPosted { get; set; }
}
