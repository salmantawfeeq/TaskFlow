using TaskFlow.Application.Interfaces.Services;

namespace TaskFlow.Web.BackgroundServices;

/// <summary>
/// Periodically invokes INotificationService.CheckAndRaiseDueDateAlertsAsync
/// to scan for tasks due soon or overdue and notify their assignees.
/// Implements the "Task Deadline Alerts" requirement without needing an
/// external scheduler (e.g. Hangfire/Quartz) for a project of this scope.
/// </summary>
public class DueDateAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DueDateAlertBackgroundService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    public DueDateAlertBackgroundService(IServiceProvider serviceProvider, ILogger<DueDateAlertBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.CheckAndRaiseDueDateAlertsAsync();

                _logger.LogInformation("Due date alert scan completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking due date alerts.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}
