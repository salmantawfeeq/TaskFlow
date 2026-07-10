using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Common;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AdminService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork uow,
        IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(AdminUserFilterDto filter)
    {
        var query = _context.Users.Include(u => u.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.FirstName)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var dto = _mapper.Map<AdminUserDto>(user);
            dto.Roles = await _userManager.GetRolesAsync(user);

            if (!string.IsNullOrEmpty(filter.Role) && !dto.Roles.Contains(filter.Role))
            {
                continue;
            }

            dtos.Add(dto);
        }

        return new PagedResult<AdminUserDto>
        {
            Items = dtos,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServiceResult> SetUserActiveStatusAsync(string userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }

        user.IsActive = isActive;
        // Also toggle Identity's own lockout so an inactive user cannot log
        // in at all (belt-and-braces alongside any [Authorize] IsActive checks).
        user.LockoutEnabled = true;
        user.LockoutEnd = isActive ? null : DateTimeOffset.MaxValue;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> AssignRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }

        if (await _userManager.IsInRoleAsync(user, role))
        {
            return ServiceResult.Failure("User already has this role.");
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> RemoveRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.Failure("User not found.");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<IReadOnlyList<SystemSettingDto>> GetSettingsAsync()
    {
        var settings = await _uow.SystemSettings.Query().OrderBy(s => s.Category).ThenBy(s => s.Key).ToListAsync();
        return _mapper.Map<List<SystemSettingDto>>(settings);
    }

    public async Task<ServiceResult> UpdateSettingAsync(string key, string value)
    {
        var setting = await _uow.SystemSettings.SingleOrDefaultAsync(s => s.Key == key);
        if (setting == null)
        {
            return ServiceResult.Failure("Setting not found.");
        }

        setting.Value = value;
        setting.UpdatedAt = DateTime.UtcNow;

        _uow.SystemSettings.Update(setting);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<PagedResult<SystemLogDto>> GetSystemLogsAsync(int pageNumber, int pageSize, string? level)
    {
        var query = _uow.SystemLogs.Query().AsQueryable();

        if (!string.IsNullOrEmpty(level))
        {
            query = query.Where(l => l.Level == level);
        }

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<SystemLogDto>
        {
            Items = _mapper.Map<List<SystemLogDto>>(logs),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<EmailLogDto>> GetEmailLogsAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _uow.EmailLogs.CountAsync();

        var logs = await _uow.EmailLogs.Query()
            .OrderByDescending(e => e.SentAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<EmailLogDto>
        {
            Items = _mapper.Map<List<EmailLogDto>>(logs),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
