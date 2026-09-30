using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Interfaces.Repositories;
using TaskFlow.Application.Interfaces.Services;
using TaskFlow.Application.Mappings;
using TaskFlow.Application.Validators;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.FileStorage;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string uploadsRootPath)
    {
        // ---- Database ----
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // ---- ASP.NET Identity ----
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password policy: reasonably strong defaults for an
                // enterprise app; adjust via configuration if needed.
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // ---- Repositories & Unit of Work ----
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ---- AutoMapper ----
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

        // ---- FluentValidation ----
        services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
        services.AddFluentValidationAutoValidation();

        // ---- Application services ----
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IAttachmentService>(sp =>
            new AttachmentService(
                sp.GetRequiredService<IUnitOfWork>(),
                sp.GetRequiredService<IMapper>(),
                uploadsRootPath));

        return services;
    }
}
