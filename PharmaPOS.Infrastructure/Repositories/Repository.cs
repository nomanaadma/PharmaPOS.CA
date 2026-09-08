using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Infrastructure.Data;

namespace PharmaPOS.Infrastructure.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
{
	private readonly DbSet<T> _dbSet = context.Set<T>();

	public IQueryable<T> Query() => _dbSet.AsQueryable();

	public IQueryable<T> AsNoTracking() => _dbSet.AsNoTracking();

	public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
	{
		return await _dbSet.FindAsync([id], ct);
	}

	public async Task<T?> FirstOrDefaultAsync(CancellationToken ct = default)
	{
		return await _dbSet.FirstOrDefaultAsync(ct);
	}

	public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
	{
		return await _dbSet.FirstOrDefaultAsync(predicate, ct);
	}

	public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
	{
		return await _dbSet.AnyAsync(predicate, ct);
	}

	public async Task<List<T>> ListAllAsync(CancellationToken ct = default)
	{
		return await _dbSet.ToListAsync(ct);
	}

	public async Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
	{
		return await _dbSet.Where(predicate).ToListAsync(ct);
	}

	public async Task<T> AddAsync(T entity, CancellationToken ct = default)
	{
		await _dbSet.AddAsync(entity, ct);
		return entity;
	}

	public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
	{
		await _dbSet.AddRangeAsync(entities, ct);
	}

	public void Update(T entity)
	{
		_dbSet.Update(entity);
	}

	public void Remove(T entity)
	{
		_dbSet.Remove(entity);
	}

	public void RemoveRange(IEnumerable<T> entities)
	{
		_dbSet.RemoveRange(entities);
	}

	public async Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
	{
		return await _dbSet.Where(predicate).ExecuteDeleteAsync(ct);
	}

	public async Task<int> SaveChangesAsync(CancellationToken ct = default)
	{
		return await context.SaveChangesAsync(ct);
	}
}
