using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;

namespace TaskFlow.Application.Interfaces.Services;

/// <summary>
/// Wraps ASP.NET Identity operations (SignInManager/UserManager) behind an
/// application-level abstraction so controllers depend on this interface,
/// not directly on Identity managers. Keeps Identity's API surface (which
/// is fairly wide) out of the Web layer's controllers.
/// </summary>
public interface IAuthService
{
    Task<ServiceResult<string>> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Simulates sending an email confirmation link and returns the token
    /// (in a real deployment this would only be emailed, never returned to
    /// the caller - here it's surfaced so the demo can display "confirmation
    /// link" directly, since no real SMTP server is configured).
    /// </summary>
    Task<ServiceResult<string>> GenerateEmailConfirmationTokenAsync(string userId);

    Task<ServiceResult> ConfirmEmailAsync(string userId, string token);

    Task<ServiceResult> LoginAsync(LoginDto dto);

    Task LogoutAsync();

    Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string email);

    Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto);

    Task<ServiceResult> ChangePasswordAsync(ChangePasswordDto dto);

    Task<UserProfileDto?> GetProfileAsync(string userId);

    Task<ServiceResult> UpdateProfileAsync(UpdateProfileDto dto);

    Task<ServiceResult> UpdateProfileImageAsync(string userId, string imagePath);
}
