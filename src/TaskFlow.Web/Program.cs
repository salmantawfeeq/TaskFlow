using Microsoft.AspNetCore.Identity;
using Serilog;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Seed;
using TaskFlow.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Serilog: structured logging to console + SQL Server (SystemLogs table),
// configured before the host builds so startup failures are captured too.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// MVC + Razor
builder.Services.AddControllersWithViews();

var uploadsRootPath = Path.Combine(builder.Environment.WebRootPath ?? "wwwroot", "uploads");
Directory.CreateDirectory(uploadsRootPath);

builder.Services.AddInfrastructure(builder.Configuration, uploadsRootPath);

builder.Services.AddHostedService<TaskFlow.Web.BackgroundServices.DueDateAlertBackgroundService>();

// Cookie/Identity application cookie settings (login paths, expiry)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// Session (used for transient UI state e.g. Kanban filter memory)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Anti-forgery: explicit header name so AJAX calls (Kanban drag/drop,
// notification polling) can attach the token via a custom header.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// Global exception handling + HSTS in production
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Database migration + seed data on startup (idempotent - safe to run
// every time the app boots, per DatabaseSeeder's internal existence checks).
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while migrating or seeding the database.");
    }
}

try
{
    Log.Information("Starting TaskFlow web application");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
