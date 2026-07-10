using System.Net;
using System.Text.Json;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Web.Middleware;

/// <summary>
/// Catches any exception that escapes controller action filters, logs it
/// (both via Serilog and into the SystemLogs table so it's visible in the
/// Admin Panel's Logs screen), and returns an appropriate response:
/// a JSON error payload for AJAX/API-style requests, or a redirect to a
/// friendly error view for normal page requests.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred processing {Path}", context.Request.Path);

        // Best-effort write to SystemLogs - if the database itself is the
        // problem, this write may also fail, so it's wrapped defensively
        // and never allowed to mask the original exception's response.
        try
        {
            using var scope = context.RequestServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.SystemLogs.Add(new Domain.Entities.SystemLog
            {
                Level = "Error",
                Message = exception.Message,
                ExceptionDetails = exception.ToString(),
                Source = context.Request.Path,
                UserId = context.User?.Identity?.IsAuthenticated == true
                    ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    : null
            });

            await dbContext.SaveChangesAsync();
        }
        catch
        {
            // Swallow secondary logging failures - the primary Serilog
            // entry above already captured the exception.
        }

        var statusCode = exception switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            BusinessRuleException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Forbidden,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        var isAjaxRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                             context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isAjaxRequest)
        {
            context.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(new
            {
                error = true,
                message = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again."
            });
            await context.Response.WriteAsync(payload);
        }
        else
        {
            context.Response.Redirect($"/Home/Error?statusCode={(int)statusCode}");
        }
    }
}
