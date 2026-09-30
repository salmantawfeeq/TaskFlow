using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Web.ViewModels;

namespace TaskFlow.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterAsync(new RegisterDto
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Password = model.Password
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        var tokenResult = await _authService.GenerateEmailConfirmationTokenAsync(result.Data!);
        if (tokenResult.Succeeded)
        {
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail), "Account",
                new { userId = result.Data, token = tokenResult.Data }, Request.Scheme);

            TempData["ConfirmationLink"] = confirmationLink;
        }

        return RedirectToAction(nameof(RegisterConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterConfirmation()
    {
        ViewBag.ConfirmationLink = TempData["ConfirmationLink"];
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _authService.ConfirmEmailAsync(userId, token);
        ViewBag.Success = result.Succeeded;

        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(new LoginDto
        {
            Email = model.Email,
            Password = model.Password,
            RememberMe = model.RememberMe
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var tokenResult = await _authService.GeneratePasswordResetTokenAsync(model.Email);

        // Always show the same confirmation regardless of whether the
        // email exists, to prevent user enumeration attacks.
        if (tokenResult.Succeeded)
        {
            var resetLink = Url.Action(
                nameof(ResetPassword), "Account",
                new { email = model.Email, token = tokenResult.Data }, Request.Scheme);

            TempData["ResetLink"] = resetLink;
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        ViewBag.ResetLink = TempData["ResetLink"];
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string email, string token) =>
        View(new ResetPasswordViewModel { Email = email, Token = token });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = model.Email,
            Token = model.Token,
            NewPassword = model.NewPassword
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation() => View();

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _authService.ChangePasswordAsync(new ChangePasswordDto
        {
            UserId = userId,
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        TempData["StatusMessage"] = "Your password has been changed successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var profile = await _authService.GetProfileAsync(userId);

        if (profile == null)
        {
            return NotFound();
        }

        return View(new ProfileViewModel
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email,
            JobTitle = profile.JobTitle,
            Bio = profile.Bio,
            ProfileImagePath = profile.ProfileImagePath,
            DepartmentName = profile.DepartmentName,
            Roles = profile.Roles,
            CreatedAt = profile.CreatedAt,
            LastLoginAt = profile.LastLoginAt
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await _authService.UpdateProfileAsync(new UpdateProfileDto
        {
            UserId = userId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            JobTitle = model.JobTitle,
            Bio = model.Bio
        });

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        TempData["StatusMessage"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (file == null || file.Length == 0)
        {
            TempData["StatusMessage"] = "Please select a valid image file.";
            return RedirectToAction(nameof(Profile));
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension) || file.Length > 2 * 1024 * 1024)
        {
            TempData["StatusMessage"] = "Only JPG/PNG/GIF images up to 2MB are allowed.";
            return RedirectToAction(nameof(Profile));
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(uploadsPath);

        var fullPath = Path.Combine(uploadsPath, fileName);
        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/avatars/{fileName}";
        await _authService.UpdateProfileImageAsync(userId, relativePath);

        TempData["StatusMessage"] = "Profile picture updated.";
        return RedirectToAction(nameof(Profile));
    }
}
