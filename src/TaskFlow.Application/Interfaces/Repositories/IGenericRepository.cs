using System.Linq.Expressions;

namespace TaskFlow.Application.Interfaces.Repositories;

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

    IQueryable<T> Query();
}
