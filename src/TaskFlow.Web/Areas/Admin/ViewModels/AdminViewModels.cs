using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Common;

namespace TaskFlow.Web.Areas.Admin.ViewModels;

public class AdminUsersViewModel
{
    public PagedResult<AdminUserDto> Users { get; set; } = new();
    public AdminUserFilterDto Filter { get; set; } = new();
}

public class AdminCategoryViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    [Display(Name = "Color")]
    public string ColorHex { get; set; } = "#6366F1";

    public bool IsActive { get; set; } = true;
}

public class AdminSettingsViewModel
{
    public List<SystemSettingDto> Settings { get; set; } = new();
}

public class AdminLogsViewModel
{
    public PagedResult<SystemLogDto> SystemLogs { get; set; } = new();
    public PagedResult<EmailLogDto> EmailLogs { get; set; } = new();
    public string ActiveTab { get; set; } = "System";
}
