using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Web.ViewModels;

namespace TaskFlow.Web.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsController(IProjectService projectService, IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _projectService = projectService;
        _uow = uow;
        _userManager = userManager;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private bool IsManagerOrAdmin => User.IsInRole("Admin") || User.IsInRole("Manager");

    public async Task<IActionResult> Index(ProjectFilterDto filter)
    {
        var result = await _projectService.GetFilteredAsync(filter);
        var categories = await _uow.Categories.Query().Where(c => c.IsActive).ToListAsync();

        return View(new ProjectListViewModel
        {
            Projects = result,
            Filter = filter,
            Categories = categories.Select(c => (c.Id, c.Name)).ToList()
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var project = await _projectService.GetDetailAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create()
    {
        var vm = new ProjectFormViewModel { OwnerId = CurrentUserId };
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var result = await _projectService.CreateAsync(new CreateProjectDto
        {
            Name = vm.Name,
            Description = vm.Description,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            ColorHex = vm.ColorHex,
            CategoryId = vm.CategoryId,
            TeamId = vm.TeamId,
            OwnerId = string.IsNullOrEmpty(vm.OwnerId) ? CurrentUserId : vm.OwnerId,
            MemberUserIds = vm.MemberUserIds
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        TempData["StatusMessage"] = "Project created successfully.";
        return RedirectToAction(nameof(Details), new { id = result.Data });
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _projectService.GetDetailAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        var vm = new ProjectFormViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            ColorHex = project.ColorHex,
            CategoryId = project.CategoryId,
            TeamId = project.TeamId,
            OwnerId = project.OwnerId
        };

        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var result = await _projectService.UpdateAsync(new UpdateProjectDto
        {
            Id = vm.Id,
            Name = vm.Name,
            Description = vm.Description,
            Status = vm.Status,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            ColorHex = vm.ColorHex,
            CategoryId = vm.CategoryId,
            TeamId = vm.TeamId
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        TempData["StatusMessage"] = "Project updated successfully.";
        return RedirectToAction(nameof(Details), new { id = vm.Id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _projectService.DeleteAsync(id);
        TempData["StatusMessage"] = result.Succeeded ? "Project deleted." : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleArchive(int id, bool archive)
    {
        var result = await _projectService.ArchiveAsync(id, archive);
        TempData["StatusMessage"] = result.Succeeded
            ? (archive ? "Project archived." : "Project restored.")
            : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int projectId, string userId, bool asManager = false)
    {
        var result = await _projectService.AddMemberAsync(projectId, userId, asManager);
        TempData["StatusMessage"] = result.Succeeded ? "Member added." : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Details), new { id = projectId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int projectId, string userId)
    {
        var result = await _projectService.RemoveMemberAsync(projectId, userId);
        TempData["StatusMessage"] = result.Succeeded ? "Member removed." : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Details), new { id = projectId });
    }

    private async Task PopulateDropdownsAsync(ProjectFormViewModel vm)
    {
        var categories = await _uow.Categories.Query().Where(c => c.IsActive).ToListAsync();
        vm.Categories = categories.Select(c => (c.Id, c.Name)).ToList();

        var teams = await _uow.Teams.Query().Where(t => t.IsActive).ToListAsync();
        vm.Teams = teams.Select(t => (t.Id, t.Name)).ToList();

        var users = _userManager.Users.Where(u => u.IsActive).ToList();
        vm.Users = users.Select(u => (u.Id, u.FullName)).ToList();
    }
}
