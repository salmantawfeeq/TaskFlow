using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Web.ViewModels;

public class ProjectListViewModel
{
    public PagedResult<ProjectDto> Projects { get; set; } = new();
    public ProjectFilterDto Filter { get; set; } = new();
    public List<(int Id, string Name)> Categories { get; set; } = new();
}

public class ProjectFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    [Required]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "End Date")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Color")]
    public string ColorHex { get; set; } = "#3B82F6";

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Display(Name = "Team")]
    public int? TeamId { get; set; }

    [Display(Name = "Owner")]
    public string OwnerId { get; set; } = string.Empty;

    public List<string> MemberUserIds { get; set; } = new();

    // ---- Dropdown data populated by the controller ----
    public List<(int Id, string Name)> Categories { get; set; } = new();
    public List<(int Id, string Name)> Teams { get; set; } = new();
    public List<(string Id, string Name)> Users { get; set; } = new();
}
