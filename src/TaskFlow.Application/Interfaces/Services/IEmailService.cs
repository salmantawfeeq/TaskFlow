namespace TaskFlow.Application.Interfaces.Services;

/// <summary>
/// Simulates sending an email by writing an EmailLog row instead of using
/// real SMTP (per the project spec: "Email Simulation"). If real email is
/// ever needed, only the Infrastructure implementation of this interface
/// needs to change - callers throughout the app are unaffected.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string body, string emailType);
}
