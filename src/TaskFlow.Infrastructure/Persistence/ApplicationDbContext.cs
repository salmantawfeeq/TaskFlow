using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskItemEntity = TaskFlow.Domain.Entities.TaskItem;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// Application database context. Extends IdentityDbContext so ASP.NET
/// Identity's user/role/claim tables live in the same database and same
/// migration history as the domain tables, which is the standard and
/// simplest approach for an ASP.NET Core MVC app using Identity.
///
/// IdentityDbContext&lt;ApplicationUser, IdentityRole, string&gt; gives us:
///   - AspNetUsers (backed by our ApplicationUser subtype)
///   - AspNetRoles, AspNetUserRoles, AspNetUserClaims, AspNetUserLogins,
///     AspNetUserTokens, AspNetRoleClaims
/// with a string (GUID) primary key, matching IdentityUser's default.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ---- Organization ----
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<UserInvite> UserInvites => Set<UserInvite>();

    // ---- Projects ----
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Category> Categories => Set<Category>();

    // ---- Tasks ----
    public DbSet<TaskItemEntity> TaskItems => Set<TaskItemEntity>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<TaskLabel> TaskLabels => Set<TaskLabel>();

    // ---- Collaboration ----
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    // ---- Logging / Notifications ----
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // ---- Settings ----
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must run first: sets up Identity's own table mappings.
        base.OnModelCreating(builder);

        // Apply every IEntityTypeConfiguration<T> found in this assembly
        // (one configuration class per entity - keeps OnModelCreating tidy
        // and each entity's mapping co-located in its own file).
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Rename default Identity tables to a consistent, explicit naming
        // scheme so the schema reads cleanly in SSMS alongside domain tables.
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // Global query filters: automatically exclude soft-deleted rows from
        // every query unless explicitly overridden with IgnoreQueryFilters().
        builder.Entity<Project>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<TaskItemEntity>().HasQueryFilter(t => !t.IsDeleted);
    }
}
