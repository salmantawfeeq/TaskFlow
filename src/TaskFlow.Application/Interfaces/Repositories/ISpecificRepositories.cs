using TaskFlow.Domain.Entities;
using TaskItemEntity = TaskFlow.Domain.Entities.TaskItem;

namespace TaskFlow.Application.Interfaces.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{
    /// <summary>
    /// Fetches a project including its Members, Category, Owner, Team, and
    /// Tasks navigation properties in a single query, for detail pages.
    /// </summary>
    Task<Project?> GetWithDetailsAsync(int id);
}

public interface ITaskRepository : IGenericRepository<TaskItemEntity>
{
    /// <summary>
    /// Fetches a task including Assignments, Labels, ChecklistItems,
    /// Comments, Attachments, and Activities for the task detail page.
    /// </summary>
    Task<TaskItemEntity?> GetWithDetailsAsync(int id);

    /// <summary>
    /// All tasks for a project grouped implicitly by Status (the caller
    /// groups client-side), used to render the Kanban board in one query.
    /// </summary>
    Task<IReadOnlyList<TaskItemEntity>> GetKanbanTasksAsync(int projectId);
}

public interface ICommentRepository : IGenericRepository<Comment>
{
}

public interface IAttachmentRepository : IGenericRepository<Attachment>
{
}

public interface IChecklistItemRepository : IGenericRepository<ChecklistItem>
{
}

public interface ILabelRepository : IGenericRepository<Label>
{
}

public interface IDepartmentRepository : IGenericRepository<Department>
{
}

public interface ITeamRepository : IGenericRepository<Team>
{
    Task<Team?> GetWithMembersAsync(int id);
}

public interface ICategoryRepository : IGenericRepository<Category>
{
}

public interface INotificationRepository : IGenericRepository<Notification>
{
}

public interface IActivityLogRepository : IGenericRepository<ActivityLog>
{
}

public interface ISystemLogRepository : IGenericRepository<SystemLog>
{
}

public interface IEmailLogRepository : IGenericRepository<EmailLog>
{
}

public interface ISystemSettingRepository : IGenericRepository<SystemSetting>
{
    Task<string?> GetValueAsync(string key);
}

public interface IUserInviteRepository : IGenericRepository<UserInvite>
{
}
