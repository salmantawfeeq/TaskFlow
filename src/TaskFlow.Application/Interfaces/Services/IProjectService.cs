using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs.Projects;

namespace TaskFlow.Application.Interfaces.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetFilteredAsync(ProjectFilterDto filter);

    Task<ProjectDetailDto?> GetDetailAsync(int id);

    Task<ServiceResult<int>> CreateAsync(CreateProjectDto dto);

    Task<ServiceResult> UpdateAsync(UpdateProjectDto dto);

    Task<ServiceResult> DeleteAsync(int id);

    Task<ServiceResult> ArchiveAsync(int id, bool archive);

    Task<ServiceResult> AddMemberAsync(int projectId, string userId, bool asManager);

    Task<ServiceResult> RemoveMemberAsync(int projectId, string userId);

    /// <summary>
    /// Returns lightweight (Id, Name) pairs for populating dropdowns
    /// (e.g. "assign task to project") without the overhead of full DTOs.
    /// </summary>
    Task<IReadOnlyList<(int Id, string Name)>> GetLookupListAsync();
}
