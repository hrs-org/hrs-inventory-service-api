using System.Linq.Expressions;

namespace HRS.Domain.Interfaces;

public interface ICrudRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity, object id);
    Task RemoveAsync(object id);
    Task RemoveRangeAsync(IEnumerable<object> ids);
}
