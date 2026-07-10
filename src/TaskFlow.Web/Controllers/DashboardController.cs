using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsManagerOrAdmin => User.IsInRole("Admin") || User.IsInRole("Manager");

    public async Task<IActionResult> Index()
    {
        ViewBag.Stats = await _dashboardService.GetStatsAsync(CurrentUserId, IsManagerOrAdmin);
        ViewBag.ProjectProgress = await _dashboardService.GetProjectProgressAsync(CurrentUserId, IsManagerOrAdmin);
        ViewBag.RecentActivity = await _dashboardService.GetRecentActivityAsync(10);

        return View();
    }

    /// <summary>
    /// AJAX endpoint feeding the Chart.js task-status doughnut chart.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> TaskStatusChartData()
    {
        var data = await _dashboardService.GetTaskStatusChartAsync(CurrentUserId, IsManagerOrAdmin);
        return Json(data);
    }

    /// <summary>
    /// AJAX endpoint feeding the Chart.js task-priority bar chart.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> TaskPriorityChartData()
    {
        var data = await _dashboardService.GetTaskPriorityChartAsync(CurrentUserId, IsManagerOrAdmin);
        return Json(data);
    }

    /// <summary>
    /// AJAX endpoint feeding the Chart.js "tasks completed over time" line chart.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CompletionTrendChartData(int days = 14)
    {
        var data = await _dashboardService.GetTasksCompletedTimeSeriesAsync(days);
        return Json(data);
    }
}
