namespace TaskFlow.Infrastructure.Persistence.Seed;

public static class SeedConstants
{
    // ---- Role names (must match Domain.Enums.SystemRole names exactly) ----
    public const string RoleAdmin = "Admin";
    public const string RoleManager = "Manager";
    public const string RoleEmployee = "Employee";

    // ---- Well-known demo user ids ----
    public const string AdminUserId = "10000000-0000-0000-0000-000000000001";
    public const string ManagerUserId1 = "10000000-0000-0000-0000-000000000002";
    public const string ManagerUserId2 = "10000000-0000-0000-0000-000000000003";
    public const string EmployeeUserId1 = "10000000-0000-0000-0000-000000000004";
    public const string EmployeeUserId2 = "10000000-0000-0000-0000-000000000005";
    public const string EmployeeUserId3 = "10000000-0000-0000-0000-000000000006";
    public const string EmployeeUserId4 = "10000000-0000-0000-0000-000000000007";

    /// <summary>
    /// Default password for ALL seeded demo accounts.
    /// Documented clearly in README - for local/demo use only, never for production.
    /// </summary>
    public const string DefaultDemoPassword = "Demo@12345";
}
