using Microsoft.EntityFrameworkCore.Storage;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Domain.Common;
using MediatR;

namespace PharmaPOS.Infrastructure.Common.Persistence;

public class UnitOfWork(ApplicationDbContext context, IPublisher publisher) : IUnitOfWork
{
	private readonly Dictionary<Type, object> _repositories = new();

	public IRepository<T> Repository<T>() where T : class
	{
		if (_repositories.TryGetValue(typeof(T), out var repo))
		{
			return (IRepository<T>)repo;
		}

		var newRepo = new Repository<T>(context);
		_repositories[typeof(T)] = newRepo;
		return newRepo;
	}

	public async Task<int> SaveChangesAsync(CancellationToken ct = default)
	{
		
		// get hold of all the domain events
		var domainEvents = context.ChangeTracker.Entries<AggregateRoot>()
			.Select(entry => entry.Entity.ClearDomainEvents())
			.SelectMany(x => x)
			.ToList();

		// store them in the http context for later if user is waiting online
		if (context.IsUserWaitingOnline())
		{
			context.AddDomainEventsToOfflineProcessingQueue(domainEvents);
		}
		else
		{
			await PublishDomainEvents(publisher, domainEvents);
		}

		return await context.SaveChangesAsync(ct);
		
	}

	public async Task<IDbTransactionProxy> BeginTransactionAsync(CancellationToken ct = default)
	{
		var transaction = await context.Database.BeginTransactionAsync(ct);
		return new DbTransactionProxy(transaction);
	}
	
	private static async Task PublishDomainEvents(IPublisher publisher, List<IDomainEvent> domainEvents)
	{
		foreach (var domainEvent in domainEvents)
		{
			await publisher.Publish(domainEvent);
		}
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
