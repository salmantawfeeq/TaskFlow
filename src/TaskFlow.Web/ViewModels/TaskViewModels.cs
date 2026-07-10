using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Web.ViewModels;

public class KanbanBoardViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectColorHex { get; set; } = string.Empty;
    public List<TaskItemDto> Tasks { get; set; } = new();
    public List<(string Id, string Name)> Users { get; set; } = new();
}

public class TaskFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Required]
    public int ProjectId { get; set; }

    public int? ParentTaskId { get; set; }

    public List<string> AssigneeUserIds { get; set; } = new();

    public List<int> LabelIds { get; set; } = new();

    // ---- Dropdown data ----
    public List<(int Id, string Name)> Projects { get; set; } = new();
    public List<(string Id, string Name)> Users { get; set; } = new();
    public List<(int Id, string Name)> Labels { get; set; } = new();
}

public class CalendarViewModel
{
    public int? ProjectId { get; set; }
    public List<(int Id, string Name)> Projects { get; set; } = new();
    public List<TaskItemDto> Tasks { get; set; } = new();
}
