using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Common;
using TaskFlow.Application.DTOs.Dashboard;
using TaskFlow.Application.DTOs.Reports;
using TaskFlow.Application.DTOs.Teams;

namespace TaskFlow.Application.Interfaces.Services;

public interface ITeamService
{
    Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync();

    Task<ServiceResult<int>> CreateDepartmentAsync(string name, string? description);

    Task<IReadOnlyList<TeamDto>> GetTeamsAsync(int? departmentId = null);

    Task<TeamDetailDto?> GetTeamDetailAsync(int id);

    Task<ServiceResult<int>> CreateTeamAsync(CreateTeamDto dto);

    Task<ServiceResult> AddTeamMemberAsync(int teamId, string userId);

    Task<ServiceResult> RemoveTeamMemberAsync(int teamId, string userId);

    Task<ServiceResult> InviteUserAsync(InviteUserDto dto);

    Task<ServiceResult> AcceptInviteAsync(string token, string acceptingUserId);
}

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(string userId, bool unreadOnly = false);

    Task<int> GetUnreadCountAsync(string userId);

    Task MarkAsReadAsync(int notificationId);

    Task MarkAllAsReadAsync(string userId);

    Task NotifyAsync(
        string userId,
        Domain.Enums.NotificationType type,
        string title,
        string message,
        string? linkUrl = null,
        int? relatedTaskId = null,
        int? relatedProjectId = null);

    Task CheckAndRaiseDueDateAlertsAsync();
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(string userId, bool isManagerOrAdmin);

    Task<TaskStatusChartDto> GetTaskStatusChartAsync(string userId, bool isManagerOrAdmin);

    Task<TaskPriorityChartDto> GetTaskPriorityChartAsync(string userId, bool isManagerOrAdmin);

    Task<IReadOnlyList<ChartDataPointDto>> GetTasksCompletedTimeSeriesAsync(int days);

    Task<IReadOnlyList<ProjectProgressDto>> GetProjectProgressAsync(string userId, bool isManagerOrAdmin);

    Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(int count);
}

public interface IReportService
{
    Task<IReadOnlyList<CompletedTasksReportDto>> GetCompletedTasksReportAsync(ReportFilterDto filter);

    Task<IReadOnlyList<ProductivityReportDto>> GetProductivityReportAsync(ReportFilterDto filter);

    Task<IReadOnlyList<EmployeePerformanceReportDto>> GetEmployeePerformanceReportAsync(ReportFilterDto filter);

    Task<byte[]> ExportToPdfAsync(string reportType, ReportFilterDto filter);

    Task<byte[]> ExportToExcelAsync(string reportType, ReportFilterDto filter);
}

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(AdminUserFilterDto filter);

    Task<ServiceResult> SetUserActiveStatusAsync(string userId, bool isActive);

    Task<ServiceResult> AssignRoleAsync(string userId, string role);

    Task<ServiceResult> RemoveRoleAsync(string userId, string role);

    Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync();

    Task<ServiceResult> UpdateSettingAsync(string key, string value);

    Task<PagedResult<SystemLogDto>> GetSystemLogsAsync(int pageNumber, int pageSize, string? level);

    Task<PagedResult<EmailLogDto>> GetEmailLogsAsync(int pageNumber, int pageSize);
}
