using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Infrastructure.Data;

namespace PharmaPOS.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context, IServiceProvider serviceProvider) : IUnitOfWork
{
	public IRepository<T> Repository<T>() where T : class
	{
		return serviceProvider.GetRequiredService<IRepository<T>>();
	}

	public async Task<int> SaveChangesAsync(CancellationToken ct = default)
	{
		return await context.SaveChangesAsync(ct);
	}

	public async Task<IDbTransactionProxy> BeginTransactionAsync(CancellationToken ct = default)
	{
		var transaction = await context.Database.BeginTransactionAsync(ct);
		return new DbTransactionProxy(transaction);
	}
}

public class DbTransactionProxy(IDbContextTransaction transaction) : IDbTransactionProxy
{
	public async Task CommitAsync(CancellationToken ct = default)
	{
		await transaction.CommitAsync(ct);
	}

	public async Task RollbackAsync(CancellationToken ct = default)
	{
		await transaction.RollbackAsync(ct);
	}

	public async ValueTask DisposeAsync()
	{
		await transaction.DisposeAsync();
		GC.SuppressFinalize(this);
	}
}
