using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Web.Areas.Admin.ViewModels;

namespace TaskFlow.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class LogsController : Controller
{
    private readonly IAdminService _adminService;

    public LogsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(string tab = "System", int pageNumber = 1, string? level = null)
    {
        var vm = new AdminLogsViewModel { ActiveTab = tab };

        if (tab == "Email")
        {
            vm.EmailLogs = await _adminService.GetEmailLogsAsync(pageNumber, 20);
        }
        else
        {
            vm.SystemLogs = await _adminService.GetSystemLogsAsync(pageNumber, 20, level);
        }

        return View(vm);
    }
}
