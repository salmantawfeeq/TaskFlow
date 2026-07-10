using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces.Repositories;
using TaskItemEntity = TaskFlow.Domain.Entities.TaskItem;

namespace TaskFlow.Infrastructure.Repositories;

public class TaskRepository : GenericRepository<TaskItemEntity>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TaskItemEntity?> GetWithDetailsAsync(int id) =>
        await DbSet
            .Include(t => t.Project)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Assignments).ThenInclude(a => a.User)
            .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
            .Include(t => t.ChecklistItems)
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt)).ThenInclude(c => c.User)
            .Include(t => t.Attachments).ThenInclude(a => a.UploadedByUser)
            .Include(t => t.Activities.OrderByDescending(a => a.CreatedAt)).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IReadOnlyList<TaskItemEntity>> GetKanbanTasksAsync(int projectId) =>
        await DbSet
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Project)
            .Include(t => t.Assignments).ThenInclude(a => a.User)
            .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .OrderBy(t => t.BoardOrder)
            .ToListAsync();
}
