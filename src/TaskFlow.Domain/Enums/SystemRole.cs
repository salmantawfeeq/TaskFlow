namespace TaskFlow.Domain.Enums;

/// <summary>
/// Application-wide roles enforced via ASP.NET Identity.
/// Stored as Identity Role names (strings) in AspNetRoles; this enum exists
/// so role names are referenced as constants throughout the codebase instead
/// of "magic strings" scattered across controllers/services.
/// </summary>
public enum SystemRole
{
    Admin = 1,
    Manager = 2,
    Employee = 3
}
