namespace TaskFlow.Application.DTOs.Teams;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public int TeamCount { get; set; }
}

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string TeamLeadId { get; set; } = string.Empty;
    public string TeamLeadName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int ActiveProjectCount { get; set; }
}

public class TeamDetailDto : TeamDto
{
    public List<TeamMemberDto> Members { get; set; } = new();
}

public class TeamMemberDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool IsTeamLead { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class CreateTeamDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DepartmentId { get; set; }
    public string TeamLeadId { get; set; } = string.Empty;
    public List<string> MemberUserIds { get; set; } = new();
}

public class InviteUserDto
{
    public string Email { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public string InvitedByUserId { get; set; } = string.Empty;
}
