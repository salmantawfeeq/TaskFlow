using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Web.ViewModels;

namespace TaskFlow.Web.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public TasksController(
        ITaskService taskService,
        IProjectService projectService,
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager)
    {
        _taskService = taskService;
        _projectService = projectService;
        _uow = uow;
        _userManager = userManager;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Kanban(int projectId)
    {
        var project = await _projectService.GetDetailAsync(projectId);
        if (project == null)
        {
            return NotFound();
        }

        var tasks = await _taskService.GetKanbanBoardAsync(projectId);
        var users = _userManager.Users.Where(u => u.IsActive).ToList();

        return View(new KanbanBoardViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            ProjectColorHex = project.ColorHex,
            Tasks = tasks.ToList(),
            Users = users.Select(u => (u.Id, u.FullName)).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveTask([FromBody] MoveTaskRequest request)
    {
        var result = await _taskService.MoveTaskAsync(new MoveTaskDto
        {
            TaskId = request.TaskId,
            NewStatus = request.NewStatus,
            NewBoardOrder = request.NewBoardOrder,
            ChangedByUserId = CurrentUserId
        });

        return Json(new { success = result.Succeeded, errors = result.Errors });
    }

    public class MoveTaskRequest
    {
        public int TaskId { get; set; }
        public Domain.Enums.TaskStatus NewStatus { get; set; }
        public int NewBoardOrder { get; set; }
    }

    public async Task<IActionResult> Calendar(int? projectId)
    {
        var rangeStart = DateTime.UtcNow.AddMonths(-1);
        var rangeEnd = DateTime.UtcNow.AddMonths(2);

        var tasks = await _taskService.GetCalendarTasksAsync(projectId, rangeStart, rangeEnd);
        var projects = await _projectService.GetLookupListAsync();

        return View(new CalendarViewModel
        {
            ProjectId = projectId,
            Projects = projects.ToList(),
            Tasks = tasks.ToList()
        });
    }

    public async Task<IActionResult> MyTasks(TaskFilterDto filter)
    {
        filter.AssigneeUserId = CurrentUserId;
        var result = await _taskService.GetFilteredAsync(filter);
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var task = await _taskService.GetDetailAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }

    public async Task<IActionResult> Create(int projectId)
    {
        var vm = new TaskFormViewModel { ProjectId = projectId };
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var result = await _taskService.CreateAsync(new CreateTaskDto
        {
            Title = vm.Title,
            Description = vm.Description,
            Priority = vm.Priority,
            DueDate = vm.DueDate,
            StartDate = vm.StartDate,
            ProjectId = vm.ProjectId,
            ParentTaskId = vm.ParentTaskId,
            CreatedByUserId = CurrentUserId,
            AssigneeUserIds = vm.AssigneeUserIds,
            LabelIds = vm.LabelIds
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        TempData["StatusMessage"] = "Task created successfully.";
        return RedirectToAction(nameof(Kanban), new { projectId = vm.ProjectId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var task = await _taskService.GetDetailAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        var vm = new TaskFormViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            DueDate = task.DueDate,
            StartDate = task.StartDate,
            ProjectId = task.ProjectId
        };

        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TaskFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var result = await _taskService.UpdateAsync(new UpdateTaskDto
        {
            Id = vm.Id,
            Title = vm.Title,
            Description = vm.Description,
            Priority = vm.Priority,
            DueDate = vm.DueDate,
            StartDate = vm.StartDate
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        TempData["StatusMessage"] = "Task updated successfully.";
        return RedirectToAction(nameof(Details), new { id = vm.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int projectId)
    {
        await _taskService.DeleteAsync(id, CurrentUserId);
        TempData["StatusMessage"] = "Task deleted.";
        return RedirectToAction(nameof(Kanban), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignUser(int taskId, string userId)
    {
        var result = await _taskService.AssignUserAsync(taskId, userId, CurrentUserId);
        return Json(new { success = result.Succeeded, errors = result.Errors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnassignUser(int taskId, string userId)
    {
        var result = await _taskService.UnassignUserAsync(taskId, userId);
        return Json(new { success = result.Succeeded, errors = result.Errors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddChecklistItem(int taskId, string text)
    {
        var result = await _taskService.AddChecklistItemAsync(taskId, text);
        return Json(new { success = result.Succeeded, id = result.Data, errors = result.Errors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleChecklistItem(int itemId, bool isCompleted)
    {
        var result = await _taskService.ToggleChecklistItemAsync(itemId, isCompleted);
        return Json(new { success = result.Succeeded, errors = result.Errors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveChecklistItem(int itemId)
    {
        var result = await _taskService.RemoveChecklistItemAsync(itemId);
        return Json(new { success = result.Succeeded, errors = result.Errors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int taskId, string content, int? parentCommentId)
    {
        var result = await _taskService.AddCommentAsync(taskId, CurrentUserId, content, parentCommentId);
        return Json(new { success = result.Succeeded, id = result.Data, errors = result.Errors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var result = await _taskService.DeleteCommentAsync(commentId, CurrentUserId);
        return Json(new { success = result.Succeeded, errors = result.Errors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLabel(int taskId, int labelId)
    {
        var result = await _taskService.AddLabelAsync(taskId, labelId);
        return Json(new { success = result.Succeeded, errors = result.Errors });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLabel(int taskId, int labelId)
    {
        var result = await _taskService.RemoveLabelAsync(taskId, labelId);
        return Json(new { success = result.Succeeded, errors = result.Errors });
    }

    private async Task PopulateDropdownsAsync(TaskFormViewModel vm)
    {
        var projects = await _projectService.GetLookupListAsync();
        vm.Projects = projects.ToList();

        var users = _userManager.Users.Where(u => u.IsActive).ToList();
        vm.Users = users.Select(u => (u.Id, u.FullName)).ToList();

        var labels = await _uow.Labels.Query().Where(l => l.ProjectId == vm.ProjectId).ToListAsync();
        vm.Labels = labels.Select(l => (l.Id, l.Name)).ToList();
    }
}
