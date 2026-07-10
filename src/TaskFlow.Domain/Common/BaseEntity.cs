namespace TaskFlow.Domain.Common;

/// <summary>
/// Base class for all domain entities that have an integer identity key.
/// Centralizes audit fields (CreatedAt/UpdatedAt) so every entity gets them
/// automatically without repeating the same properties everywhere.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    /// <summary>
    /// UTC timestamp set once when the entity is first created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp updated every time the entity is modified.
    /// Null until the first update occurs.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Base class for entities that support soft-delete (archiving) instead of
/// being physically removed from the database. Used by Project, TaskItem, etc.
/// </summary>
public abstract class AuditableSoftDeleteEntity : BaseEntity
{
    /// <summary>
    /// When true, the entity is excluded from normal queries via a global
    /// query filter but remains in the database for audit/history purposes.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }
}
