using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Web.Controllers;

[Authorize]
public class AttachmentsController : Controller
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadForTask(int taskId, IFormFile file)
    {
        var result = await _attachmentService.UploadAsync(file, EntityRelationType.TaskItem, taskId, CurrentUserId);

        TempData["StatusMessage"] = result.Succeeded
            ? "File uploaded successfully."
            : string.Join(" ", result.Errors);

        return RedirectToAction("Details", "Tasks", new { id = taskId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadForProject(int projectId, IFormFile file)
    {
        var result = await _attachmentService.UploadAsync(file, EntityRelationType.Project, projectId, CurrentUserId);

        TempData["StatusMessage"] = result.Succeeded
            ? "File uploaded successfully."
            : string.Join(" ", result.Errors);

        return RedirectToAction("Details", "Projects", new { id = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string returnController, int returnId)
    {
        await _attachmentService.DeleteAsync(id, CurrentUserId);
        TempData["StatusMessage"] = "Attachment removed.";

        return returnController == "Projects"
            ? RedirectToAction("Details", "Projects", new { id = returnId })
            : RedirectToAction("Details", "Tasks", new { id = returnId });
    }
}
