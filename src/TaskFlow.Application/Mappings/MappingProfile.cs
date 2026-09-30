using AutoMapper;
using TaskFlow.Application.DTOs.Common;
using TaskFlow.Application.DTOs.Dashboard;
using TaskFlow.Application.DTOs.Projects;
using TaskFlow.Application.DTOs.Tasks;
using TaskFlow.Application.DTOs.Teams;
using TaskFlow.Domain.Entities;
using TaskItemEntity = TaskFlow.Domain.Entities.TaskItem;

namespace TaskFlow.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Project, ProjectDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner.FullName))
            .ForMember(d => d.TeamName, o => o.MapFrom(s => s.Team != null ? s.Team.Name : null))
            .ForMember(d => d.TotalTasks, o => o.MapFrom(s => s.Tasks.Count))
            .ForMember(d => d.CompletedTasks, o => o.MapFrom(s => s.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done)))
            .ForMember(d => d.MemberCount, o => o.MapFrom(s => s.Members.Count));

        CreateMap<Project, ProjectDetailDto>()
            .IncludeBase<Project, ProjectDto>()
            .ForMember(d => d.Members, o => o.MapFrom(s => s.Members));

        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.ProfileImagePath, o => o.MapFrom(s => s.User.ProfileImagePath));

        CreateMap<TaskItemEntity, TaskItemDto>()
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Project.Name))
            .ForMember(d => d.ProjectColorHex, o => o.MapFrom(s => s.Project.ColorHex))
            .ForMember(d => d.CommentCount, o => o.MapFrom(s => s.Comments.Count))
            .ForMember(d => d.AttachmentCount, o => o.MapFrom(s => s.Attachments.Count))
            .ForMember(d => d.Assignees, o => o.MapFrom(s => s.Assignments))
            .ForMember(d => d.Labels, o => o.MapFrom(s => s.TaskLabels.Select(tl => tl.Label)));

        CreateMap<TaskItemEntity, TaskDetailDto>()
            .IncludeBase<TaskItemEntity, TaskItemDto>()
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedByUser.FullName))
            .ForMember(d => d.ChecklistItems, o => o.MapFrom(s => s.ChecklistItems.OrderBy(c => c.SortOrder)))
            .ForMember(d => d.Attachments, o => o.MapFrom(s => s.Attachments))
            .ForMember(d => d.Activities, o => o.MapFrom(s => s.Activities.OrderByDescending(a => a.CreatedAt)));

        CreateMap<TaskAssignment, TaskAssigneeDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.ProfileImagePath, o => o.MapFrom(s => s.User.ProfileImagePath));

        CreateMap<Label, TaskLabelDto>();

        CreateMap<ChecklistItem, ChecklistItemDto>();

        CreateMap<Comment, CommentDto>()
            .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.UserProfileImagePath, o => o.MapFrom(s => s.User.ProfileImagePath))
            .ForMember(d => d.Replies, o => o.MapFrom(s => s.Replies));

        CreateMap<Attachment, AttachmentDto>()
            .ForMember(d => d.UploadedByName, o => o.MapFrom(s => s.UploadedByUser.FullName));

        CreateMap<ActivityLog, ActivityLogDto>()
            .ForMember(d => d.ActionType, o => o.MapFrom(s => s.ActionType.ToString()))
            .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.UserProfileImagePath, o => o.MapFrom(s => s.User.ProfileImagePath));

        CreateMap<ActivityLog, RecentActivityDto>()
            .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.UserProfileImagePath, o => o.MapFrom(s => s.User.ProfileImagePath));

        CreateMap<Notification, NotificationDto>();

        CreateMap<Department, DepartmentDto>()
            .ForMember(d => d.MemberCount, o => o.MapFrom(s => s.Members.Count))
            .ForMember(d => d.TeamCount, o => o.MapFrom(s => s.Teams.Count));

        CreateMap<Team, TeamDto>()
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name))
            .ForMember(d => d.TeamLeadName, o => o.MapFrom(s => s.TeamLead.FullName))
            .ForMember(d => d.MemberCount, o => o.MapFrom(s => s.Members.Count))
            .ForMember(d => d.ActiveProjectCount, o => o.MapFrom(s => s.Projects.Count(p => !p.IsArchived)));

        CreateMap<Team, TeamDetailDto>()
            .IncludeBase<Team, TeamDto>()
            .ForMember(d => d.Members, o => o.MapFrom(s => s.Members));

        CreateMap<TeamMember, TeamMemberDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.JobTitle, o => o.MapFrom(s => s.User.JobTitle))
            .ForMember(d => d.ProfileImagePath, o => o.MapFrom(s => s.User.ProfileImagePath));

        CreateMap<ApplicationUser, AdminUserDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.Name : null))
            .ForMember(d => d.Roles, o => o.Ignore()); // populated separately via UserManager.GetRolesAsync

        CreateMap<SystemSetting, SystemSettingDto>();
        CreateMap<SystemLog, SystemLogDto>();
        CreateMap<EmailLog, EmailLogDto>();
    }
}
