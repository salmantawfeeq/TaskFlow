using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Teams;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public TeamService(IUnitOfWork uow, IMapper mapper, IEmailService emailService)
    {
        _uow = uow;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync()
    {
        var departments = await _uow.Departments.Query()
            .Include(d => d.Members)
            .Include(d => d.Teams)
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();

        return _mapper.Map<List<DepartmentDto>>(departments);
    }

    public async Task<ServiceResult<int>> CreateDepartmentAsync(string name, string? description)
    {
        if (await _uow.Departments.AnyAsync(d => d.Name == name))
        {
            return ServiceResult<int>.Failure("A department with this name already exists.");
        }

        var department = new Department { Name = name, Description = description };
        await _uow.Departments.AddAsync(department);
        await _uow.SaveChangesAsync();

        return ServiceResult<int>.Success(department.Id);
    }

    public async Task<IReadOnlyList<TeamDto>> GetTeamsAsync(int? departmentId = null)
    {
        var query = _uow.Teams.Query()
            .Include(t => t.Department)
            .Include(t => t.TeamLead)
            .Include(t => t.Members)
            .Include(t => t.Projects)
            .Where(t => t.IsActive);

        if (departmentId.HasValue)
        {
            query = query.Where(t => t.DepartmentId == departmentId.Value);
        }

        var teams = await query.OrderBy(t => t.Name).ToListAsync();
        return _mapper.Map<List<TeamDto>>(teams);
    }

    public async Task<TeamDetailDto?> GetTeamDetailAsync(int id)
    {
        var team = await _uow.Teams.GetWithMembersAsync(id);
        return team == null ? null : _mapper.Map<TeamDetailDto>(team);
    }

    public async Task<ServiceResult<int>> CreateTeamAsync(CreateTeamDto dto)
    {
        var team = new Team
        {
            Name = dto.Name,
            Description = dto.Description,
            DepartmentId = dto.DepartmentId,
            TeamLeadId = dto.TeamLeadId
        };

        team.Members.Add(new TeamMember { UserId = dto.TeamLeadId, IsTeamLead = true });

        foreach (var userId in dto.MemberUserIds.Where(id => id != dto.TeamLeadId).Distinct())
        {
            team.Members.Add(new TeamMember { UserId = userId });
        }

        await _uow.Teams.AddAsync(team);
        await _uow.SaveChangesAsync();

        return ServiceResult<int>.Success(team.Id);
    }

    public async Task<ServiceResult> AddTeamMemberAsync(int teamId, string userId)
    {
        var team = await _uow.Teams.GetWithMembersAsync(teamId);
        if (team == null)
        {
            return ServiceResult.Failure("Team not found.");
        }

        if (team.Members.Any(m => m.UserId == userId))
        {
            return ServiceResult.Failure("User is already a member of this team.");
        }

        team.Members.Add(new TeamMember { UserId = userId });
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveTeamMemberAsync(int teamId, string userId)
    {
        var team = await _uow.Teams.GetWithMembersAsync(teamId);
        if (team == null)
        {
            return ServiceResult.Failure("Team not found.");
        }

        var member = team.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
        {
            return ServiceResult.Failure("User is not a member of this team.");
        }

        team.Members.Remove(member);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> InviteUserAsync(InviteUserDto dto)
    {
        var token = Guid.NewGuid().ToString("N");

        var invite = new UserInvite
        {
            Email = dto.Email,
            Token = token,
            TeamId = dto.TeamId,
            InvitedByUserId = dto.InvitedByUserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _uow.UserInvites.AddAsync(invite);
        await _uow.SaveChangesAsync();

        await _emailService.SendAsync(
            dto.Email,
            "You've been invited to join TaskFlow",
            $"You have been invited to join TaskFlow. Use invitation code {token} when registering to join your team automatically.",
            "TeamInvite");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> AcceptInviteAsync(string token, string acceptingUserId)
    {
        var invite = await _uow.UserInvites.SingleOrDefaultAsync(i => i.Token == token);
        if (invite == null)
        {
            return ServiceResult.Failure("Invalid invitation code.");
        }

        if (invite.IsAccepted)
        {
            return ServiceResult.Failure("This invitation has already been used.");
        }

        if (invite.ExpiresAt < DateTime.UtcNow)
        {
            return ServiceResult.Failure("This invitation has expired.");
        }

        invite.IsAccepted = true;
        invite.AcceptedAt = DateTime.UtcNow;
        _uow.UserInvites.Update(invite);

        if (invite.TeamId.HasValue)
        {
            var team = await _uow.Teams.GetWithMembersAsync(invite.TeamId.Value);
            if (team != null && !team.Members.Any(m => m.UserId == acceptingUserId))
            {
                team.Members.Add(new TeamMember { UserId = acceptingUserId });
            }
        }

        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }
}
