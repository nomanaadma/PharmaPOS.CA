using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using PharmaPOS.Application.Common.Events;
using PharmaPOS.Application.Common.Interfaces;
using PharmaPOS.Domain.Common;
using MediatR;

namespace PharmaPOS.Infrastructure.Common.Persistence;

public class UnitOfWork(ApplicationDbContext context, IPublisher publisher, IServiceScopeFactory scopeFactory) : IUnitOfWork
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
		var domainEntities = context.ChangeTracker
			.Entries<AggregateRoot>()
			.Where(x => x.Entity.DomainEvents.Count != 0)
			.ToList();

		var domainEvents = domainEntities
			.SelectMany(x => x.Entity.DomainEvents)
			.ToList();

		domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

		var notifications = domainEvents.Select(e =>
		{
			var type = typeof(DomainEventNotification<>).MakeGenericType(e.GetType());
			return (IDomainEventNotification)Activator.CreateInstance(type, e)!;
		}).ToList();

		foreach (var notification in notifications.Where(n => n.DomainEvent is not IAsyncDomainEvent))
			await publisher.Publish(notification, ct);

		var result = await context.SaveChangesAsync(ct);

		var asyncNotifications = notifications.Where(n => n.DomainEvent is IAsyncDomainEvent).ToList();
		if (asyncNotifications.Count != 0)
		{
			_ = Task.Run(async () =>
			{
				using var scope = scopeFactory.CreateScope();
				var scopedPublisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
				foreach (var asyncNotification in asyncNotifications)
				{
					try { await scopedPublisher.Publish(asyncNotification, ct); }
					catch { /* Background tasks should log their own exceptions */ }
				}
			}, ct);
		}

		return result;
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
