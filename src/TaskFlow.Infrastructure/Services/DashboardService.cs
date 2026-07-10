using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Dashboard;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public DashboardService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    /// <summary>
    /// Managers/Admins see stats across all projects; Employees see stats
    /// scoped to projects they are a member of. Centralizing this scoping
    /// rule here means every dashboard query applies it consistently.
    /// </summary>
    private IQueryable<Domain.Entities.Project> ScopedProjects(string userId, bool isManagerOrAdmin)
    {
        var query = _uow.Projects.Query();
        return isManagerOrAdmin
            ? query
            : query.Where(p => p.Members.Any(m => m.UserId == userId) || p.OwnerId == userId);
    }

    public async Task<DashboardStatsDto> GetStatsAsync(string userId, bool isManagerOrAdmin)
    {
        var projects = ScopedProjects(userId, isManagerOrAdmin);
        var projectIds = await projects.Select(p => p.Id).ToListAsync();

        var tasksQuery = _uow.Tasks.Query().Where(t => projectIds.Contains(t.ProjectId));

        var totalTasks = await tasksQuery.CountAsync();
        var completedTasks = await tasksQuery.CountAsync(t => t.Status == Domain.Enums.TaskStatus.Done);
        var now = DateTime.UtcNow;
        var overdueTasks = await tasksQuery.CountAsync(t => t.DueDate != null && t.DueDate < now && t.Status != Domain.Enums.TaskStatus.Done);
        var tasksAssignedToMe = await _uow.Tasks.Query().CountAsync(t => t.Assignments.Any(a => a.UserId == userId) && t.Status != Domain.Enums.TaskStatus.Done);

        var teamMembersCount = await _uow.Departments.Query().SelectMany(d => d.Members).Select(m => m.Id).Distinct().CountAsync();

        return new DashboardStatsDto
        {
            TotalProjects = await projects.CountAsync(),
            ActiveProjects = await projects.CountAsync(p => p.Status == Domain.Enums.ProjectStatus.Active),
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            OverdueTasks = overdueTasks,
            TasksAssignedToMe = tasksAssignedToMe,
            TeamMembersCount = teamMembersCount,
            OverallCompletionRate = totalTasks == 0 ? 0 : Math.Round(completedTasks * 100.0 / totalTasks, 1)
        };
    }

    public async Task<TaskStatusChartDto> GetTaskStatusChartAsync(string userId, bool isManagerOrAdmin)
    {
        var projectIds = await ScopedProjects(userId, isManagerOrAdmin).Select(p => p.Id).ToListAsync();
        var tasks = _uow.Tasks.Query().Where(t => projectIds.Contains(t.ProjectId));

        return new TaskStatusChartDto
        {
            ToDo = await tasks.CountAsync(t => t.Status == Domain.Enums.TaskStatus.ToDo),
            InProgress = await tasks.CountAsync(t => t.Status == Domain.Enums.TaskStatus.InProgress),
            InReview = await tasks.CountAsync(t => t.Status == Domain.Enums.TaskStatus.InReview),
            Done = await tasks.CountAsync(t => t.Status == Domain.Enums.TaskStatus.Done),
            Blocked = await tasks.CountAsync(t => t.Status == Domain.Enums.TaskStatus.Blocked)
        };
    }

    public async Task<TaskPriorityChartDto> GetTaskPriorityChartAsync(string userId, bool isManagerOrAdmin)
    {
        var projectIds = await ScopedProjects(userId, isManagerOrAdmin).Select(p => p.Id).ToListAsync();
        var tasks = _uow.Tasks.Query().Where(t => projectIds.Contains(t.ProjectId));

        return new TaskPriorityChartDto
        {
            Low = await tasks.CountAsync(t => t.Priority == Domain.Enums.TaskPriority.Low),
            Medium = await tasks.CountAsync(t => t.Priority == Domain.Enums.TaskPriority.Medium),
            High = await tasks.CountAsync(t => t.Priority == Domain.Enums.TaskPriority.High),
            Critical = await tasks.CountAsync(t => t.Priority == Domain.Enums.TaskPriority.Critical)
        };
    }

    public async Task<IReadOnlyList<ChartDataPointDto>> GetTasksCompletedTimeSeriesAsync(int days)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        var completedTasks = await _uow.Tasks.Query()
            .Where(t => t.CompletedAt != null && t.CompletedAt >= startDate)
            .Select(t => t.CompletedAt!.Value.Date)
            .ToListAsync();

        var grouped = completedTasks
            .GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new List<ChartDataPointDto>();
        for (var day = startDate; day <= DateTime.UtcNow.Date; day = day.AddDays(1))
        {
            result.Add(new ChartDataPointDto
            {
                Label = day.ToString("MMM dd"),
                Value = grouped.TryGetValue(day, out var count) ? count : 0
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<ProjectProgressDto>> GetProjectProgressAsync(string userId, bool isManagerOrAdmin)
    {
        var projects = await ScopedProjects(userId, isManagerOrAdmin)
            .Include(p => p.Tasks)
            .Where(p => !p.IsArchived)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        return projects.Select(p => new ProjectProgressDto
        {
            ProjectId = p.Id,
            ProjectName = p.Name,
            ColorHex = p.ColorHex,
            ProgressPercentage = p.ProgressPercentage,
            TotalTasks = p.Tasks.Count,
            CompletedTasks = p.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done)
        }).ToList();
    }

    public async Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(int count)
    {
        var activities = await _uow.ActivityLogs.Query()
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<RecentActivityDto>>(activities);
    }
}
