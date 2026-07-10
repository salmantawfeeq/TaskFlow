using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Teams;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Web.ViewModels;

namespace TaskFlow.Web.Controllers;

[Authorize]
public class TeamsController : Controller
{
    private readonly ITeamService _teamService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeamsController(ITeamService teamService, UserManager<ApplicationUser> userManager)
    {
        _teamService = teamService;
        _userManager = userManager;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var departments = await _teamService.GetDepartmentsAsync();
        var teams = await _teamService.GetTeamsAsync();

        return View(new TeamsIndexViewModel
        {
            Departments = departments.ToList(),
            Teams = teams.ToList()
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var team = await _teamService.GetTeamDetailAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        return View(team);
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create()
    {
        var vm = new CreateTeamViewModel();
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTeamViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var result = await _teamService.CreateTeamAsync(new CreateTeamDto
        {
            Name = vm.Name,
            Description = vm.Description,
            DepartmentId = vm.DepartmentId,
            TeamLeadId = vm.TeamLeadId,
            MemberUserIds = vm.MemberUserIds
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        TempData["StatusMessage"] = "Team created successfully.";
        return RedirectToAction(nameof(Details), new { id = result.Data });
    }

    [Authorize(Roles = "Admin,Manager")]
    public IActionResult CreateDepartment() => View(new CreateDepartmentViewModel());

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepartment(CreateDepartmentViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _teamService.CreateDepartmentAsync(vm.Name, vm.Description);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return View(vm);
        }

        TempData["StatusMessage"] = "Department created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int teamId, string userId)
    {
        var result = await _teamService.AddTeamMemberAsync(teamId, userId);
        TempData["StatusMessage"] = result.Succeeded ? "Member added." : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Details), new { id = teamId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int teamId, string userId)
    {
        var result = await _teamService.RemoveTeamMemberAsync(teamId, userId);
        TempData["StatusMessage"] = result.Succeeded ? "Member removed." : string.Join(" ", result.Errors);
        return RedirectToAction(nameof(Details), new { id = teamId });
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Invite()
    {
        var teams = await _teamService.GetTeamsAsync();
        return View(new InviteUserViewModel { Teams = teams.Select(t => (t.Id, t.Name)).ToList() });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(InviteUserViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var teams = await _teamService.GetTeamsAsync();
            vm.Teams = teams.Select(t => (t.Id, t.Name)).ToList();
            return View(vm);
        }

        var result = await _teamService.InviteUserAsync(new InviteUserDto
        {
            Email = vm.Email,
            TeamId = vm.TeamId,
            InvitedByUserId = CurrentUserId
        });

        TempData["StatusMessage"] = result.Succeeded
            ? $"Invitation sent to {vm.Email} (simulated email logged)."
            : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(CreateTeamViewModel vm)
    {
        var departments = await _teamService.GetDepartmentsAsync();
        vm.Departments = departments.Select(d => (d.Id, d.Name)).ToList();

        var users = _userManager.Users.Where(u => u.IsActive).ToList();
        vm.Users = users.Select(u => (u.Id, u.FullName)).ToList();
    }
}
