using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ApplicationDbContext _context;

    public EmailService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendAsync(string toEmail, string subject, string body, string emailType)
    {
        // No real SMTP call - this is a deliberate simulation per spec.
        // In a production deployment, this method would be swapped for a
        // real implementation using SendGrid/SMTP/etc, with the interface
        // (IEmailService) staying unchanged for all callers.
        _context.EmailLogs.Add(new EmailLog
        {
            ToEmail = toEmail,
            Subject = subject,
            Body = body,
            EmailType = emailType,
            IsSimulated = true,
            SentAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}
