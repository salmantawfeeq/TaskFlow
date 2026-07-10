using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Web.Areas.Admin.ViewModels;

namespace TaskFlow.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly IAdminService _adminService;

    public SettingsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _adminService.GetSettingsAsync();
        return View(new AdminSettingsViewModel { Settings = settings.ToList() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string key, string value)
    {
        var result = await _adminService.UpdateSettingAsync(key, value);
        TempData["StatusMessage"] = result.Succeeded ? "Setting updated." : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Index));
    }
}
