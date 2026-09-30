using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;

namespace TaskFlow.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult<string>> RegisterAsync(RegisterDto dto);

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
