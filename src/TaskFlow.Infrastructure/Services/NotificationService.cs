using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs.Common;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public NotificationService(ApplicationDbContext context, IEmailService emailService, IMapper mapper)
    {
        _context = context;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(string userId, bool unreadOnly = false)
    {
        var query = _context.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task<int> GetUnreadCountAsync(string userId) =>
        await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null || notification.IsRead) return;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task NotifyAsync(
        string userId,
        NotificationType type,
        string title,
        string message,
        string? linkUrl = null,
        int? relatedTaskId = null,
        int? relatedProjectId = null)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            LinkUrl = linkUrl,
            RelatedTaskItemId = relatedTaskId,
            RelatedProjectId = relatedProjectId
        });

        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        if (user?.Email != null)
        {
            await _emailService.SendAsync(user.Email, title, message, type.ToString());
        }
    }

    public async Task CheckAndRaiseDueDateAlertsAsync()
    {
        var thresholdHoursSetting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "Notifications.DueSoonThresholdHours");

        var thresholdHours = 48;
        if (thresholdHoursSetting != null && int.TryParse(thresholdHoursSetting.Value, out var parsed))
        {
            thresholdHours = parsed;
        }

        var now = DateTime.UtcNow;
        var dueSoonCutoff = now.AddHours(thresholdHours);

        // Tasks due soon (and not yet done) - notify each assignee once.
        var dueSoonTasks = await _context.TaskItems
            .Where(t => t.DueDate != null
                        && t.DueDate <= dueSoonCutoff
                        && t.DueDate >= now
                        && t.Status != Domain.Enums.TaskStatus.Done)
            .Include(t => t.Assignments)
            .ToListAsync();

        foreach (var task in dueSoonTasks)
        {
            foreach (var assignment in task.Assignments)
            {
                var alreadyNotified = await _context.Notifications.AnyAsync(n =>
                    n.RelatedTaskItemId == task.Id &&
                    n.UserId == assignment.UserId &&
                    n.Type == NotificationType.TaskDueSoon);

                if (!alreadyNotified)
                {
                    await NotifyAsync(
                        assignment.UserId,
                        NotificationType.TaskDueSoon,
                        "Task due soon",
                        $"\"{task.Title}\" is due on {task.DueDate:MMM dd, yyyy}.",
                        $"/Tasks/Details/{task.Id}",
                        relatedTaskId: task.Id);
                }
            }
        }

        // Overdue tasks - notify each assignee once.
        var overdueTasks = await _context.TaskItems
            .Where(t => t.DueDate != null && t.DueDate < now && t.Status != Domain.Enums.TaskStatus.Done)
            .Include(t => t.Assignments)
            .ToListAsync();

        foreach (var task in overdueTasks)
        {
            foreach (var assignment in task.Assignments)
            {
                var alreadyNotified = await _context.Notifications.AnyAsync(n =>
                    n.RelatedTaskItemId == task.Id &&
                    n.UserId == assignment.UserId &&
                    n.Type == NotificationType.TaskOverdue);

                if (!alreadyNotified)
                {
                    await NotifyAsync(
                        assignment.UserId,
                        NotificationType.TaskOverdue,
                        "Task overdue",
                        $"\"{task.Title}\" was due on {task.DueDate:MMM dd, yyyy} and is still not complete.",
                        $"/Tasks/Details/{task.Id}",
                        relatedTaskId: task.Id);
                }
            }
        }
    }
}
