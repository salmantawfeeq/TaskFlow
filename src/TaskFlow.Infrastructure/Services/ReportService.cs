using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskFlow.Application.DTOs.Reports;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow)
    {
        _uow = uow;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<IReadOnlyList<CompletedTasksReportDto>> GetCompletedTasksReportAsync(ReportFilterDto filter)
    {
        var query = _uow.Tasks.Query()
            .Include(t => t.Project)
            .Include(t => t.Assignments).ThenInclude(a => a.User)
            .Where(t => t.Status == Domain.Enums.TaskStatus.Done &&
                        t.CompletedAt != null &&
                        t.CompletedAt >= filter.StartDate &&
                        t.CompletedAt <= filter.EndDate);

        if (filter.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == filter.ProjectId.Value);
        }

        var tasks = await query.ToListAsync();

        return tasks.Select(t => new CompletedTasksReportDto
        {
            TaskId = t.Id,
            Title = t.Title,
            ProjectName = t.Project.Name,
            CompletedByName = t.Assignments.FirstOrDefault()?.User.FullName ?? t.CreatedByUser?.FullName ?? "Unassigned",
            CompletedAt = t.CompletedAt,
            Priority = t.Priority.ToString(),
            DaysToComplete = t.StartDate.HasValue && t.CompletedAt.HasValue
                ? (int)(t.CompletedAt.Value - t.StartDate.Value).TotalDays
                : 0
        }).OrderByDescending(r => r.CompletedAt).ToList();
    }

    public async Task<IReadOnlyList<ProductivityReportDto>> GetProductivityReportAsync(ReportFilterDto filter)
    {
        var assignments = await _uow.Tasks.Query()
            .Include(t => t.Assignments).ThenInclude(a => a.User)
            .Where(t => t.CreatedAt >= filter.StartDate && t.CreatedAt <= filter.EndDate)
            .SelectMany(t => t.Assignments, (task, assignment) => new { task, assignment })
            .ToListAsync();

        var now = DateTime.UtcNow;

        var grouped = assignments
            .GroupBy(x => new { x.assignment.UserId, x.assignment.User.FirstName, x.assignment.User.LastName })
            .Select(g =>
            {
                var tasksAssigned = g.Count();
                var completed = g.Where(x => x.task.Status == Domain.Enums.TaskStatus.Done).ToList();
                var overdue = g.Count(x => x.task.DueDate != null && x.task.DueDate < now && x.task.Status != Domain.Enums.TaskStatus.Done);

                var avgDays = completed
                    .Where(x => x.task.StartDate.HasValue && x.task.CompletedAt.HasValue)
                    .Select(x => (x.task.CompletedAt!.Value - x.task.StartDate!.Value).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average();

                return new ProductivityReportDto
                {
                    UserId = g.Key.UserId,
                    UserFullName = $"{g.Key.FirstName} {g.Key.LastName}",
                    TasksAssigned = tasksAssigned,
                    TasksCompleted = completed.Count,
                    CompletionRate = tasksAssigned == 0 ? 0 : Math.Round(completed.Count * 100.0 / tasksAssigned, 1),
                    AverageCompletionDays = Math.Round(avgDays, 1),
                    OverdueTasks = overdue
                };
            })
            .OrderByDescending(r => r.CompletionRate)
            .ToList();

        return grouped;
    }

    public async Task<IReadOnlyList<EmployeePerformanceReportDto>> GetEmployeePerformanceReportAsync(ReportFilterDto filter)
    {
        var users = await _uow.Departments.Query()
            .SelectMany(d => d.Members)
            .Where(u => !filter.DepartmentId.HasValue || u.DepartmentId == filter.DepartmentId.Value)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var result = new List<EmployeePerformanceReportDto>();

        foreach (var user in users)
        {
            var assignments = await _uow.Tasks.Query()
                .Where(t => t.Assignments.Any(a => a.UserId == user.Id))
                .ToListAsync();

            var completedOnTime = assignments.Count(t =>
                t.Status == Domain.Enums.TaskStatus.Done &&
                t.CompletedAt.HasValue && t.DueDate.HasValue &&
                t.CompletedAt <= t.DueDate);

            var completedLate = assignments.Count(t =>
                t.Status == Domain.Enums.TaskStatus.Done &&
                t.CompletedAt.HasValue && t.DueDate.HasValue &&
                t.CompletedAt > t.DueDate);

            var overdueNow = assignments.Count(t =>
                t.DueDate != null && t.DueDate < now && t.Status != Domain.Enums.TaskStatus.Done);

            var commentsPosted = await _uow.Comments.CountAsync(c => c.UserId == user.Id);

            result.Add(new EmployeePerformanceReportDto
            {
                UserId = user.Id,
                UserFullName = user.FullName,
                JobTitle = user.JobTitle,
                DepartmentName = user.Department?.Name,
                TotalTasksAssigned = assignments.Count,
                TasksCompletedOnTime = completedOnTime,
                TasksCompletedLate = completedLate,
                TasksOverdueNow = overdueNow,
                OnTimeCompletionRate = assignments.Count == 0 ? 0 : Math.Round(completedOnTime * 100.0 / assignments.Count, 1),
                CommentsPosted = commentsPosted
            });
        }

        return result.OrderByDescending(r => r.OnTimeCompletionRate).ToList();
    }

    public async Task<byte[]> ExportToPdfAsync(string reportType, ReportFilterDto filter)
    {
        var title = reportType switch
        {
            "CompletedTasks" => "Completed Tasks Report",
            "Productivity" => "Productivity Report",
            "EmployeePerformance" => "Employee Performance Report",
            _ => "Report"
        };

        var rows = await BuildReportRowsAsync(reportType, filter);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("TaskFlow").FontSize(18).Bold();
                    col.Item().Text(title).FontSize(14);
                    col.Item().Text($"Period: {filter.StartDate:MMM dd, yyyy} - {filter.EndDate:MMM dd, yyyy}").FontSize(9);
                    col.Item().PaddingTop(10).LineHorizontal(1);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        for (var i = 0; i < (rows.Count > 0 ? rows[0].Length : 1); i++)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    if (rows.Count > 0)
                    {
                        table.Header(header =>
                        {
                            foreach (var cell in rows[0])
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(cell).Bold();
                            }
                        });

                        foreach (var row in rows.Skip(1))
                        {
                            foreach (var cell in row)
                            {
                                table.Cell().Padding(5).Text(cell);
                            }
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.UtcNow.ToString("MMM dd, yyyy HH:mm")).Bold();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportToExcelAsync(string reportType, ReportFilterDto filter)
    {
        var rows = await BuildReportRowsAsync(reportType, filter);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(reportType);

        for (var r = 0; r < rows.Count; r++)
        {
            for (var c = 0; c < rows[r].Length; c++)
            {
                var cell = worksheet.Cell(r + 1, c + 1);
                cell.Value = rows[r][c];
                if (r == 0)
                {
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Builds a simple string[][] table (header row + data rows) shared by
    /// both the PDF and Excel exporters, so the two output formats always
    /// present identical data without duplicating the query/shaping logic.
    /// </summary>
    private async Task<List<string[]>> BuildReportRowsAsync(string reportType, ReportFilterDto filter)
    {
        var rows = new List<string[]>();

        switch (reportType)
        {
            case "CompletedTasks":
                rows.Add(new[] { "Task", "Project", "Completed By", "Completed At", "Priority", "Days To Complete" });
                foreach (var r in await GetCompletedTasksReportAsync(filter))
                {
                    rows.Add(new[]
                    {
                        r.Title, r.ProjectName, r.CompletedByName,
                        r.CompletedAt?.ToString("MMM dd, yyyy") ?? "-",
                        r.Priority, r.DaysToComplete.ToString()
                    });
                }
                break;

            case "Productivity":
                rows.Add(new[] { "User", "Tasks Assigned", "Tasks Completed", "Completion Rate %", "Avg Days", "Overdue" });
                foreach (var r in await GetProductivityReportAsync(filter))
                {
                    rows.Add(new[]
                    {
                        r.UserFullName, r.TasksAssigned.ToString(), r.TasksCompleted.ToString(),
                        r.CompletionRate.ToString("0.0"), r.AverageCompletionDays.ToString("0.0"),
                        r.OverdueTasks.ToString()
                    });
                }
                break;

            case "EmployeePerformance":
                rows.Add(new[] { "Employee", "Department", "Total Assigned", "On Time", "Late", "Overdue Now", "On-Time Rate %" });
                foreach (var r in await GetEmployeePerformanceReportAsync(filter))
                {
                    rows.Add(new[]
                    {
                        r.UserFullName, r.DepartmentName ?? "-", r.TotalTasksAssigned.ToString(),
                        r.TasksCompletedOnTime.ToString(), r.TasksCompletedLate.ToString(),
                        r.TasksOverdueNow.ToString(), r.OnTimeCompletionRate.ToString("0.0")
                    });
                }
                break;
        }

        return rows;
    }
}
