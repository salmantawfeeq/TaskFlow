using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.DTOs.Teams;

namespace TaskFlow.Web.ViewModels;

public class TeamsIndexViewModel
{
    public List<DepartmentDto> Departments { get; set; } = new();
    public List<TeamDto> Teams { get; set; } = new();
}

public class CreateTeamViewModel
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required, Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Required, Display(Name = "Team Lead")]
    public string TeamLeadId { get; set; } = string.Empty;

    public List<string> MemberUserIds { get; set; } = new();

    public List<(int Id, string Name)> Departments { get; set; } = new();
    public List<(string Id, string Name)> Users { get; set; } = new();
}

public class CreateDepartmentViewModel
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}

public class InviteUserViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Team")]
    public int? TeamId { get; set; }

    public List<(int Id, string Name)> Teams { get; set; } = new();
}
