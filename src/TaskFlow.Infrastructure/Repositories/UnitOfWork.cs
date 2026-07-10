using Microsoft.EntityFrameworkCore.Storage;
using TaskFlow.Application.Interfaces.Repositories;

namespace TaskFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUnitOfWork. Lazily constructs each repository
/// on first access, sharing the same DbContext instance (and therefore the
/// same change-tracker and transaction) across all of them.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    private IProjectRepository? _projects;
    private ITaskRepository? _tasks;
    private ICommentRepository? _comments;
    private IAttachmentRepository? _attachments;
    private IChecklistItemRepository? _checklistItems;
    private ILabelRepository? _labels;
    private IDepartmentRepository? _departments;
    private ITeamRepository? _teams;
    private ICategoryRepository? _categories;
    private INotificationRepository? _notifications;
    private IActivityLogRepository? _activityLogs;
    private ISystemLogRepository? _systemLogs;
    private IEmailLogRepository? _emailLogs;
    private ISystemSettingRepository? _systemSettings;
    private IUserInviteRepository? _userInvites;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);
    public ICommentRepository Comments => _comments ??= new CommentRepository(_context);
    public IAttachmentRepository Attachments => _attachments ??= new AttachmentRepository(_context);
    public IChecklistItemRepository ChecklistItems => _checklistItems ??= new ChecklistItemRepository(_context);
    public ILabelRepository Labels => _labels ??= new LabelRepository(_context);
    public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(_context);
    public ITeamRepository Teams => _teams ??= new TeamRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
    public IActivityLogRepository ActivityLogs => _activityLogs ??= new ActivityLogRepository(_context);
    public ISystemLogRepository SystemLogs => _systemLogs ??= new SystemLogRepository(_context);
    public IEmailLogRepository EmailLogs => _emailLogs ??= new EmailLogRepository(_context);
    public ISystemSettingRepository SystemSettings => _systemSettings ??= new SystemSettingRepository(_context);
    public IUserInviteRepository UserInvites => _userInvites ??= new UserInviteRepository(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
