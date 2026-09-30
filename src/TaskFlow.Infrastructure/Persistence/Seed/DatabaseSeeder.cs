using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskItemEntity = TaskFlow.Domain.Entities.TaskItem;

namespace TaskFlow.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        var users = await SeedUsersAsync(userManager);
        var departments = await SeedDepartmentsAsync(context);
        var teams = await SeedTeamsAsync(context, departments, users);
        var categories = await SeedCategoriesAsync(context);
        var projects = await SeedProjectsAsync(context, categories, teams, users);
        await SeedTasksAsync(context, projects, users);
        await SeedSystemSettingsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { SeedConstants.RoleAdmin, SeedConstants.RoleManager, SeedConstants.RoleEmployee };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task<Dictionary<string, ApplicationUser>> SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var usersToCreate = new (string Id, string Email, string First, string Last, string Title, string Role)[]
        {
            (SeedConstants.AdminUserId, "admin@taskflow.demo", "Sarah", "Admin", "System Administrator", SeedConstants.RoleAdmin),
            (SeedConstants.ManagerUserId1, "manager1@taskflow.demo", "Michael", "Chen", "Engineering Manager", SeedConstants.RoleManager),
            (SeedConstants.ManagerUserId2, "manager2@taskflow.demo", "Emma", "Rodriguez", "Product Manager", SeedConstants.RoleManager),
            (SeedConstants.EmployeeUserId1, "employee1@taskflow.demo", "James", "Wilson", "Senior Developer", SeedConstants.RoleEmployee),
            (SeedConstants.EmployeeUserId2, "employee2@taskflow.demo", "Olivia", "Taylor", "UI/UX Designer", SeedConstants.RoleEmployee),
            (SeedConstants.EmployeeUserId3, "employee3@taskflow.demo", "Ahmed", "Hassan", "Backend Developer", SeedConstants.RoleEmployee),
            (SeedConstants.EmployeeUserId4, "employee4@taskflow.demo", "Sophia", "Martinez", "QA Engineer", SeedConstants.RoleEmployee),
        };

        var result = new Dictionary<string, ApplicationUser>();

        foreach (var u in usersToCreate)
        {
            var existing = await userManager.FindByEmailAsync(u.Email);
            if (existing != null)
            {
                result[u.Email] = existing;
                continue;
            }

            var user = new ApplicationUser
            {
                Id = u.Id,
                UserName = u.Email,
                Email = u.Email,
                EmailConfirmed = true, // demo accounts skip the email confirmation step
                FirstName = u.First,
                LastName = u.Last,
                JobTitle = u.Title,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, SeedConstants.DefaultDemoPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, u.Role);
                result[u.Email] = user;
            }
        }

        return result;
    }

    private static async Task<Dictionary<string, Department>> SeedDepartmentsAsync(ApplicationDbContext context)
    {
        if (await context.Departments.AnyAsync())
        {
            return await context.Departments.ToDictionaryAsync(d => d.Name, d => d);
        }

        var departments = new List<Department>
        {
            new() { Name = "Engineering", Description = "Software development and technical operations" },
            new() { Name = "Product & Design", Description = "Product management and UX/UI design" },
            new() { Name = "Quality Assurance", Description = "Testing and quality control" }
        };

        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();

        return departments.ToDictionary(d => d.Name, d => d);
    }

    private static async Task<Dictionary<string, Team>> SeedTeamsAsync(
        ApplicationDbContext context,
        Dictionary<string, Department> departments,
        Dictionary<string, ApplicationUser> users)
    {
        if (await context.Teams.AnyAsync())
        {
            return await context.Teams.ToDictionaryAsync(t => t.Name, t => t);
        }

        var teams = new List<Team>
        {
            new()
            {
                Name = "Backend Team",
                Description = "API and server-side development",
                DepartmentId = departments["Engineering"].Id,
                TeamLeadId = users["manager1@taskflow.demo"].Id
            },
            new()
            {
                Name = "Frontend & Design Team",
                Description = "UI implementation and design system",
                DepartmentId = departments["Product & Design"].Id,
                TeamLeadId = users["manager2@taskflow.demo"].Id
            },
            new()
            {
                Name = "QA Team",
                Description = "Test planning and execution",
                DepartmentId = departments["Quality Assurance"].Id,
                TeamLeadId = users["manager1@taskflow.demo"].Id
            }
        };

        context.Teams.AddRange(teams);
        await context.SaveChangesAsync();

        // Add team members
        context.TeamMembers.AddRange(
            new TeamMember { TeamId = teams[0].Id, UserId = users["manager1@taskflow.demo"].Id, IsTeamLead = true },
            new TeamMember { TeamId = teams[0].Id, UserId = users["employee1@taskflow.demo"].Id },
            new TeamMember { TeamId = teams[0].Id, UserId = users["employee3@taskflow.demo"].Id },
            new TeamMember { TeamId = teams[1].Id, UserId = users["manager2@taskflow.demo"].Id, IsTeamLead = true },
            new TeamMember { TeamId = teams[1].Id, UserId = users["employee2@taskflow.demo"].Id },
            new TeamMember { TeamId = teams[2].Id, UserId = users["manager1@taskflow.demo"].Id, IsTeamLead = true },
            new TeamMember { TeamId = teams[2].Id, UserId = users["employee4@taskflow.demo"].Id }
        );
        await context.SaveChangesAsync();

        return teams.ToDictionary(t => t.Name, t => t);
    }

    private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return await context.Categories.ToDictionaryAsync(c => c.Name, c => c);
        }

        var categories = new List<Category>
        {
            new() { Name = "Software Development", ColorHex = "#4F46E5" },
            new() { Name = "Marketing Campaign", ColorHex = "#EC4899" },
            new() { Name = "Internal Operations", ColorHex = "#10B981" },
            new() { Name = "Client Project", ColorHex = "#F59E0B" }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        return categories.ToDictionary(c => c.Name, c => c);
    }

    private static async Task<List<Project>> SeedProjectsAsync(
        ApplicationDbContext context,
        Dictionary<string, Category> categories,
        Dictionary<string, Team> teams,
        Dictionary<string, ApplicationUser> users)
    {
        if (await context.Projects.AnyAsync())
        {
            return await context.Projects.ToListAsync();
        }

        var projects = new List<Project>
        {
            new()
            {
                Name = "TaskFlow Mobile App",
                Description = "Native mobile application for iOS and Android with offline support.",
                Status = ProjectStatus.Active,
                ColorHex = "#3B82F6",
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(2),
                CategoryId = categories["Software Development"].Id,
                TeamId = teams["Backend Team"].Id,
                OwnerId = users["manager1@taskflow.demo"].Id
            },
            new()
            {
                Name = "Website Redesign",
                Description = "Complete overhaul of the marketing website with new design system.",
                Status = ProjectStatus.Active,
                ColorHex = "#EC4899",
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow.AddMonths(1),
                CategoryId = categories["Marketing Campaign"].Id,
                TeamId = teams["Frontend & Design Team"].Id,
                OwnerId = users["manager2@taskflow.demo"].Id
            },
            new()
            {
                Name = "Q3 Internal Tooling",
                Description = "Automate internal reporting and reduce manual ops work.",
                Status = ProjectStatus.Planning,
                ColorHex = "#10B981",
                StartDate = DateTime.UtcNow.AddDays(-10),
                CategoryId = categories["Internal Operations"].Id,
                TeamId = teams["QA Team"].Id,
                OwnerId = users["manager1@taskflow.demo"].Id
            },
            new()
            {
                Name = "Acme Corp Integration",
                Description = "Third-party API integration project for a key enterprise client.",
                Status = ProjectStatus.OnHold,
                ColorHex = "#F59E0B",
                StartDate = DateTime.UtcNow.AddMonths(-3),
                CategoryId = categories["Client Project"].Id,
                TeamId = teams["Backend Team"].Id,
                OwnerId = users["manager1@taskflow.demo"].Id
            }
        };

        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();

        // Add project members for the first two active projects
        context.ProjectMembers.AddRange(
            new ProjectMember { ProjectId = projects[0].Id, UserId = users["manager1@taskflow.demo"].Id, IsProjectManager = true },
            new ProjectMember { ProjectId = projects[0].Id, UserId = users["employee1@taskflow.demo"].Id },
            new ProjectMember { ProjectId = projects[0].Id, UserId = users["employee3@taskflow.demo"].Id },
            new ProjectMember { ProjectId = projects[1].Id, UserId = users["manager2@taskflow.demo"].Id, IsProjectManager = true },
            new ProjectMember { ProjectId = projects[1].Id, UserId = users["employee2@taskflow.demo"].Id }
        );
        await context.SaveChangesAsync();

        return projects;
    }

    private static async Task SeedTasksAsync(
        ApplicationDbContext context,
        List<Project> projects,
        Dictionary<string, ApplicationUser> users)
    {
        if (await context.TaskItems.AnyAsync())
        {
            return;
        }

        var mobileProject = projects[0];
        var websiteProject = projects[1];

        var tasks = new List<TaskItemEntity>
        {
            new()
            {
                Title = "Design onboarding flow wireframes",
                Description = "Create low-fidelity wireframes for the new user onboarding experience.",
                Status = Domain.Enums.TaskStatus.Done,
                Priority = TaskPriority.High,
                ProjectId = mobileProject.Id,
                CreatedByUserId = users["manager1@taskflow.demo"].Id,
                DueDate = DateTime.UtcNow.AddDays(-5),
                CompletedAt = DateTime.UtcNow.AddDays(-6),
                BoardOrder = 1
            },
            new()
            {
                Title = "Implement push notification service",
                Description = "Integrate Firebase Cloud Messaging for task deadline alerts.",
                Status = Domain.Enums.TaskStatus.InProgress,
                Priority = TaskPriority.Critical,
                ProjectId = mobileProject.Id,
                CreatedByUserId = users["manager1@taskflow.demo"].Id,
                DueDate = DateTime.UtcNow.AddDays(3),
                BoardOrder = 1
            },
            new()
            {
                Title = "Fix login screen validation bug",
                Description = "Password field allows submission with fewer than 8 characters.",
                Status = Domain.Enums.TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                ProjectId = mobileProject.Id,
                CreatedByUserId = users["employee1@taskflow.demo"].Id,
                DueDate = DateTime.UtcNow.AddDays(7),
                BoardOrder = 1
            },
            new()
            {
                Title = "Write API documentation",
                Status = Domain.Enums.TaskStatus.ToDo,
                Priority = TaskPriority.Low,
                ProjectId = mobileProject.Id,
                CreatedByUserId = users["manager1@taskflow.demo"].Id,
                BoardOrder = 2
            },
            new()
            {
                Title = "Review pull request #142",
                Status = Domain.Enums.TaskStatus.InReview,
                Priority = TaskPriority.High,
                ProjectId = mobileProject.Id,
                CreatedByUserId = users["employee3@taskflow.demo"].Id,
                DueDate = DateTime.UtcNow.AddDays(1),
                BoardOrder = 1
            },
            new()
            {
                Title = "Create new homepage hero section",
                Description = "Design and implement the new hero banner with animated gradient.",
                Status = Domain.Enums.TaskStatus.InProgress,
                Priority = TaskPriority.High,
                ProjectId = websiteProject.Id,
                CreatedByUserId = users["manager2@taskflow.demo"].Id,
                DueDate = DateTime.UtcNow.AddDays(4),
                BoardOrder = 1
            },
            new()
            {
                Title = "Audit site accessibility (WCAG 2.1)",
                Status = Domain.Enums.TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                ProjectId = websiteProject.Id,
                CreatedByUserId = users["manager2@taskflow.demo"].Id,
                DueDate = DateTime.UtcNow.AddDays(-2), // intentionally overdue, for dashboard demo
                BoardOrder = 1
            },
            new()
            {
                Title = "Set up analytics tracking",
                Status = Domain.Enums.TaskStatus.Blocked,
                Priority = TaskPriority.Medium,
                ProjectId = websiteProject.Id,
                CreatedByUserId = users["employee2@taskflow.demo"].Id,
                BoardOrder = 1
            }
        };

        context.TaskItems.AddRange(tasks);
        await context.SaveChangesAsync();

        // Assign users to a few tasks
        context.TaskAssignments.AddRange(
            new TaskAssignment { TaskItemId = tasks[1].Id, UserId = users["employee3@taskflow.demo"].Id, AssignedByUserId = users["manager1@taskflow.demo"].Id },
            new TaskAssignment { TaskItemId = tasks[2].Id, UserId = users["employee1@taskflow.demo"].Id, AssignedByUserId = users["manager1@taskflow.demo"].Id },
            new TaskAssignment { TaskItemId = tasks[4].Id, UserId = users["employee1@taskflow.demo"].Id, AssignedByUserId = users["manager1@taskflow.demo"].Id },
            new TaskAssignment { TaskItemId = tasks[5].Id, UserId = users["employee2@taskflow.demo"].Id, AssignedByUserId = users["manager2@taskflow.demo"].Id },
            new TaskAssignment { TaskItemId = tasks[6].Id, UserId = users["employee2@taskflow.demo"].Id, AssignedByUserId = users["manager2@taskflow.demo"].Id }
        );

        // Sample checklist items on the in-progress push notification task
        context.ChecklistItems.AddRange(
            new ChecklistItem { TaskItemId = tasks[1].Id, Text = "Register Firebase project", IsCompleted = true, SortOrder = 1 },
            new ChecklistItem { TaskItemId = tasks[1].Id, Text = "Implement device token registration", IsCompleted = true, SortOrder = 2 },
            new ChecklistItem { TaskItemId = tasks[1].Id, Text = "Handle notification tap deep-linking", IsCompleted = false, SortOrder = 3 },
            new ChecklistItem { TaskItemId = tasks[1].Id, Text = "Write integration tests", IsCompleted = false, SortOrder = 4 }
        );

        // Sample comment thread
        context.Comments.Add(new Comment
        {
            TaskItemId = tasks[1].Id,
            UserId = users["manager1@taskflow.demo"].Id,
            Content = "Please make sure this also handles the case where notification permissions are denied."
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedSystemSettingsAsync(ApplicationDbContext context)
    {
        if (await context.SystemSettings.AnyAsync())
        {
            return;
        }

        context.SystemSettings.AddRange(
            new SystemSetting { Key = "Site.Name", Value = "TaskFlow", Category = "General", Description = "Application display name" },
            new SystemSetting { Key = "Uploads.MaxFileSizeMb", Value = "10", Category = "Uploads", Description = "Maximum attachment size in megabytes" },
            new SystemSetting { Key = "Uploads.AllowedExtensions", Value = ".jpg,.jpeg,.png,.gif,.pdf,.docx,.xlsx,.zip", Category = "Uploads", Description = "Comma-separated list of allowed file extensions" },
            new SystemSetting { Key = "Notifications.DueSoonThresholdHours", Value = "48", Category = "Notifications", Description = "Hours before a due date to trigger a deadline alert" },
            new SystemSetting { Key = "Security.MaxLoginAttempts", Value = "5", Category = "Security", Description = "Failed login attempts before lockout" }
        );

        await context.SaveChangesAsync();
    }
}
