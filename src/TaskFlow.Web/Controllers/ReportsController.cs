using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Reports;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Web.ViewModels;

namespace TaskFlow.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly IUnitOfWork _uow;

    public ReportsController(IReportService reportService, IUnitOfWork uow)
    {
        _reportService = reportService;
        _uow = uow;
    }

    public async Task<IActionResult> Index(ReportFilterDto filter, string tab = "CompletedTasks")
    {
        var projects = await _uow.Projects.Query().Select(p => new { p.Id, p.Name }).ToListAsync();

        var vm = new ReportsIndexViewModel
        {
            Filter = filter,
            ActiveTab = tab,
            Projects = projects.Select(p => (p.Id, p.Name)).ToList()
        };

        switch (tab)
        {
            case "Productivity":
                vm.Productivity = (await _reportService.GetProductivityReportAsync(filter)).ToList();
                break;
            case "EmployeePerformance":
                vm.EmployeePerformance = (await _reportService.GetEmployeePerformanceReportAsync(filter)).ToList();
                break;
            default:
                vm.CompletedTasks = (await _reportService.GetCompletedTasksReportAsync(filter)).ToList();
                break;
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(string reportType, ReportFilterDto filter)
    {
        var bytes = await _reportService.ExportToPdfAsync(reportType, filter);
        return File(bytes, "application/pdf", $"{reportType}_Report_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(string reportType, ReportFilterDto filter)
    {
        var bytes = await _reportService.ExportToExcelAsync(reportType, filter);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportType}_Report_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
