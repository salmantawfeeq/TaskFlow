using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Repositories;

public class SystemSettingRepository : GenericRepository<SystemSetting>, ISystemSettingRepository
{
    public SystemSettingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<string?> GetValueAsync(string key) =>
        (await DbSet.FirstOrDefaultAsync(s => s.Key == key))?.Value;
}

public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context) { }
}

public class AttachmentRepository : GenericRepository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(ApplicationDbContext context) : base(context) { }
}

public class ChecklistItemRepository : GenericRepository<ChecklistItem>, IChecklistItemRepository
{
    public ChecklistItemRepository(ApplicationDbContext context) : base(context) { }
}

public class LabelRepository : GenericRepository<Label>, ILabelRepository
{
    public LabelRepository(ApplicationDbContext context) : base(context) { }
}

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context) : base(context) { }
}

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }
}

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }
}

public class ActivityLogRepository : GenericRepository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(ApplicationDbContext context) : base(context) { }
}

public class SystemLogRepository : GenericRepository<SystemLog>, ISystemLogRepository
{
    public SystemLogRepository(ApplicationDbContext context) : base(context) { }
}

public class EmailLogRepository : GenericRepository<EmailLog>, IEmailLogRepository
{
    public EmailLogRepository(ApplicationDbContext context) : base(context) { }
}

public class UserInviteRepository : GenericRepository<UserInvite>, IUserInviteRepository
{
    public UserInviteRepository(ApplicationDbContext context) : base(context) { }
}
