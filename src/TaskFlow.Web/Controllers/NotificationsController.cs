using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Web.Controllers;

/// <summary>
/// Lightweight AJAX-only endpoints backing the notification bell dropdown
/// in the shared layout. Polled periodically by site.js.
/// </summary>
[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetRecent()
    {
        var items = await _notificationService.GetForUserAsync(CurrentUserId);
        var unreadCount = await _notificationService.GetUnreadCountAsync(CurrentUserId);

        return Json(new { items, unreadCount });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notificationService.MarkAllAsReadAsync(CurrentUserId);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _notificationService.GetForUserAsync(CurrentUserId);
        return View(items);
    }
}
