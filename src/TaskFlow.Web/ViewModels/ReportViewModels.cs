using TaskFlow.Application.DTOs.Reports;

namespace TaskFlow.Web.ViewModels;

public class ReportsIndexViewModel
{
    public ReportFilterDto Filter { get; set; } = new();
    public List<(int Id, string Name)> Projects { get; set; } = new();
    public string ActiveTab { get; set; } = "CompletedTasks";

    public List<CompletedTasksReportDto> CompletedTasks { get; set; } = new();
    public List<ProductivityReportDto> Productivity { get; set; } = new();
    public List<EmployeePerformanceReportDto> EmployeePerformance { get; set; } = new();
}
