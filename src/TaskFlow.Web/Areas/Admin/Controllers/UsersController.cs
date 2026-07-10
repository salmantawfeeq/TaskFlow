using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Common;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Web.Areas.Admin.ViewModels;

namespace TaskFlow.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IAdminService _adminService;

    public UsersController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(AdminUserFilterDto filter)
    {
        var result = await _adminService.GetUsersAsync(filter);
        return View(new AdminUsersViewModel { Users = result, Filter = filter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActiveStatus(string userId, bool isActive)
    {
        var result = await _adminService.SetUserActiveStatusAsync(userId, isActive);
        TempData["StatusMessage"] = result.Succeeded
            ? (isActive ? "User activated." : "User deactivated.")
            : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        var result = await _adminService.AssignRoleAsync(userId, role);
        TempData["StatusMessage"] = result.Succeeded ? $"Role '{role}' assigned." : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(string userId, string role)
    {
        var result = await _adminService.RemoveRoleAsync(userId, role);
        TempData["StatusMessage"] = result.Succeeded ? $"Role '{role}' removed." : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Index));
    }
}
