using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProjectService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProjectDto>> GetFilteredAsync(ProjectFilterDto filter)
    {
        var query = _uow.Projects.Query()
            .Include(p => p.Category)
            .Include(p => p.Owner)
            .Include(p => p.Team)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .AsQueryable();

        if (!filter.IncludeArchived)
        {
            query = query.Where(p => !p.IsArchived);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) ||
                                     (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.Status == filter.Status.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrEmpty(filter.OwnerId))
        {
            query = query.Where(p => p.OwnerId == filter.OwnerId);
        }

        query = filter.SortBy switch
        {
            "Name" => filter.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "StartDate" => filter.SortDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
            _ => filter.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<ProjectDto>
        {
            Items = _mapper.Map<List<ProjectDto>>(items),
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProjectDetailDto?> GetDetailAsync(int id)
    {
        var project = await _uow.Projects.GetWithDetailsAsync(id);
        return project == null ? null : _mapper.Map<ProjectDetailDto>(project);
    }

    public async Task<ServiceResult<int>> CreateAsync(CreateProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Status = ProjectStatus.Planning,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            ColorHex = dto.ColorHex,
            CategoryId = dto.CategoryId,
            TeamId = dto.TeamId,
            OwnerId = dto.OwnerId
        };

        // Owner is automatically a project-manager-level member; additional
        // requested members are added as regular (non-manager) members.
        // Adding them to the in-memory Members collection before the first
        // SaveChangesAsync lets EF Core insert Project + ProjectMembers
        // together as one graph, instead of two separate round trips.
        project.Members.Add(new ProjectMember { UserId = dto.OwnerId, IsProjectManager = true });

        foreach (var userId in dto.MemberUserIds.Where(id => id != dto.OwnerId).Distinct())
        {
            project.Members.Add(new ProjectMember { UserId = userId });
        }

        await _uow.Projects.AddAsync(project);
        await _uow.SaveChangesAsync();

        await LogActivityAsync(project.Id, dto.OwnerId, ActivityActionType.Created, $"created the project \"{project.Name}\".");
        await _uow.SaveChangesAsync();

        return ServiceResult<int>.Success(project.Id);
    }

    public async Task<ServiceResult> UpdateAsync(UpdateProjectDto dto)
    {
        var project = await _uow.Projects.GetByIdAsync(dto.Id);
        if (project == null)
        {
            return ServiceResult.Failure("Project not found.");
        }

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.Status = dto.Status;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.ColorHex = dto.ColorHex;
        project.CategoryId = dto.CategoryId;
        project.TeamId = dto.TeamId;
        project.UpdatedAt = DateTime.UtcNow;

        _uow.Projects.Update(project);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var project = await _uow.Projects.GetByIdAsync(id);
        if (project == null)
        {
            return ServiceResult.Failure("Project not found.");
        }

        // Soft delete: mark as deleted rather than physically removing the
        // row, so historical reports and audit trails remain intact.
        project.IsDeleted = true;
        project.DeletedAt = DateTime.UtcNow;

        _uow.Projects.Update(project);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ArchiveAsync(int id, bool archive)
    {
        var project = await _uow.Projects.GetByIdAsync(id);
        if (project == null)
        {
            return ServiceResult.Failure("Project not found.");
        }

        project.IsArchived = archive;
        project.UpdatedAt = DateTime.UtcNow;

        _uow.Projects.Update(project);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> AddMemberAsync(int projectId, string userId, bool asManager)
    {
        var project = await _uow.Projects.GetWithDetailsAsync(projectId);
        if (project == null)
        {
            return ServiceResult.Failure("Project not found.");
        }

        if (project.Members.Any(m => m.UserId == userId))
        {
            return ServiceResult.Failure("User is already a member of this project.");
        }

        project.Members.Add(new ProjectMember { ProjectId = projectId, UserId = userId, IsProjectManager = asManager });
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveMemberAsync(int projectId, string userId)
    {
        var project = await _uow.Projects.GetWithDetailsAsync(projectId);
        if (project == null)
        {
            return ServiceResult.Failure("Project not found.");
        }

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
        {
            return ServiceResult.Failure("User is not a member of this project.");
        }

        project.Members.Remove(member);
        await _uow.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<IReadOnlyList<(int Id, string Name)>> GetLookupListAsync() =>
        await _uow.Projects.Query()
            .Where(p => !p.IsArchived)
            .OrderBy(p => p.Name)
            .Select(p => new ValueTuple<int, string>(p.Id, p.Name))
            .ToListAsync();

    private async Task LogActivityAsync(int projectId, string userId, ActivityActionType actionType, string description)
    {
        await _uow.ActivityLogs.AddAsync(new ActivityLog
        {
            ProjectId = projectId,
            RelatedTo = EntityRelationType.Project,
            UserId = userId,
            ActionType = actionType,
            Description = description
        });
    }
}
