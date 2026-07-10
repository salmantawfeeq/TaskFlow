using System.Linq.Expressions;

namespace TaskFlow.Application.Interfaces.Repositories;

/// <summary>
/// Generic repository contract shared by every entity-specific repository.
/// Keeps common CRUD/query operations in one place (DRY) while specific
/// repositories (IProjectRepository, ITaskRepository, ...) add only the
/// queries unique to that entity.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);

    Task<IReadOnlyList<T>> GetAllAsync();

    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    Task AddAsync(T entity);

    Task AddRangeAsync(IEnumerable<T> entities);

    void Update(T entity);

    void Remove(T entity);

    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Returns an IQueryable for building complex, composable queries
    /// (paging, includes, projections) in the service layer without
    /// leaking EF Core's DbContext itself outside Infrastructure.
    /// </summary>
    IQueryable<T> Query();
}
