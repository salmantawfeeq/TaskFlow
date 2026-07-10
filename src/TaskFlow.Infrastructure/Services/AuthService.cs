using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public async Task<ServiceResult<string>> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
        {
            return ServiceResult<string>.Failure("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return ServiceResult<string>.Failure(result.Errors.Select(e => e.Description));
        }

        // Every new self-registered user starts as an Employee; role
        // upgrades (Manager/Admin) are performed later by an administrator.
        await _userManager.AddToRoleAsync(user, "Employee");

        return ServiceResult<string>.Success(user.Id);
    }

    public async Task<ServiceResult<string>> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<string>.Failure("User not found.");
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return ServiceResult<string>.Success(token);
    }

    public async Task<ServiceResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
        {
            return ServiceResult.Failure("Invalid email or password.");
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, dto.Password, dto.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return ServiceResult.Success();
        }

        if (result.IsLockedOut)
        {
            return ServiceResult.Failure("This account has been locked due to multiple failed login attempts. Please try again later.");
        }

        if (result.IsNotAllowed)
        {
            return ServiceResult.Failure("You must confirm your email before logging in.");
        }

        return ServiceResult.Failure("Invalid email or password.");
    }

    public async Task LogoutAsync() => await _signInManager.SignOutAsync();

    public async Task<ServiceResult<string>> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Deliberately return success-shaped failure without revealing
            // whether the email exists, to avoid user enumeration - the
            // caller (controller) should show a generic "check your email"
            // message regardless of this result.
            return ServiceResult<string>.Failure("If that email exists, a reset link has been generated.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return ServiceResult<string>.Success(token);
    }

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return ServiceResult.Failure("Invalid request.");
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            JobTitle = user.JobTitle,
            Bio = user.Bio,
            ProfileImagePath = user.ProfileImagePath,
            DepartmentName = user.Department?.Name,
            Roles = roles,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<ServiceResult> UpdateProfileAsync(UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.JobTitle = dto.JobTitle;
        user.Bio = dto.Bio;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> UpdateProfileImageAsync(string userId, string imagePath)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }

        user.ProfileImagePath = imagePath;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }
}
