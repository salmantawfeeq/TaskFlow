using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.DTOs.Projects;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public string? CategoryName { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? TeamName { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ProgressPercentage { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProjectDetailDto : ProjectDto
{
    public int? CategoryId { get; set; }
    public int? TeamId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public List<ProjectMemberDto> Members { get; set; } = new();
}

public class ProjectMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
    public bool IsProjectManager { get; set; }
    public DateTime AddedAt { get; set; }
}

public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public string ColorHex { get; set; } = "#3B82F6";
    public int? CategoryId { get; set; }
    public int? TeamId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public List<string> MemberUserIds { get; set; } = new();
}

public class UpdateProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int? TeamId { get; set; }
}

public class ProjectFilterDto
{
    public string? SearchTerm { get; set; }
    public ProjectStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    public bool IncludeArchived { get; set; } = false;
    public string? OwnerId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
