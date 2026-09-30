namespace TaskFlow.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    ICommentRepository Comments { get; }
    IAttachmentRepository Attachments { get; }
    IChecklistItemRepository ChecklistItems { get; }
    ILabelRepository Labels { get; }
    IDepartmentRepository Departments { get; }
    ITeamRepository Teams { get; }
    ICategoryRepository Categories { get; }
    INotificationRepository Notifications { get; }
    IActivityLogRepository ActivityLogs { get; }
    ISystemLogRepository SystemLogs { get; }
    IEmailLogRepository EmailLogs { get; }
    ISystemSettingRepository SystemSettings { get; }
    IUserInviteRepository UserInvites { get; }

    /// <summary>
    /// Persists all pending changes across every repository used in this
    /// unit of work, returning the number of affected rows.
    /// </summary>
    Task<int> SaveChangesAsync();

    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}
