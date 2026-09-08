using System.Linq.Expressions;

namespace PharmaPOS.Application.Interfaces;

public interface IRepository<T> where T : class
{
	IQueryable<T> Query();
	IQueryable<T> AsNoTracking();
	Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
	Task<T?> FirstOrDefaultAsync(CancellationToken ct = default);
	Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
	Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
	Task<List<T>> ListAllAsync(CancellationToken ct = default);
	Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
	Task<T> AddAsync(T entity, CancellationToken ct = default);
	Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
	void Update(T entity);
	void Remove(T entity);
	void RemoveRange(IEnumerable<T> entities);
	Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
	Task<int> SaveChangesAsync(CancellationToken ct = default);
}
