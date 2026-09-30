using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Domain.CategoryAggregate;
using PharmaPOS.Domain.Common;
using PharmaPOS.Domain.CustomerAggregate;
using PharmaPOS.Domain.MediaAggregate;
using PharmaPOS.Domain.MedicineAggregate;
using PharmaPOS.Domain.PaymentMethodAggregate;
using PharmaPOS.Domain.PurchaseAggregate;
using PharmaPOS.Domain.SettingAggregate;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Infrastructure.Common.Persistence;

public class ApplicationDbContext(
	DbContextOptions<ApplicationDbContext> options,
	IHttpContextAccessor httpContextAccessor,
	IPublisher publisher
	) : IdentityDbContext(options)
{
	public DbSet<Setting> Settings { get; set; } = null!;
	public DbSet<Media> Media { get; set; } = null!;
	public DbSet<Supplier> Suppliers { get; set; } = null!;
	public DbSet<Category> Categories { get; set; } = null!;
	public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
	public DbSet<Customer> Customers { get; set; } = null!;
	public DbSet<Medicine> Medicines { get; set; } = null!;
	public DbSet<Purchase> Purchases { get; set; } = null!;
	public DbSet<Batch> Batches { get; set; } = null!;
	public DbSet<Bill> Bills { get; set; } = null!;
	public DbSet<BillItem> BillItems { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
	}
	
	public bool IsUserWaitingOnline() => httpContextAccessor.HttpContext is not null;
	
	public void AddDomainEventsToOfflineProcessingQueue(List<IDomainEvent> domainEvents)
	{
		// fetch queue from http context or create a new queue if it doesn't exist
		var domainEventsQueue = httpContextAccessor.HttpContext!.Items
			.TryGetValue("DomainEventsQueue", out var value) && value is Queue<IDomainEvent> existingDomainEvents
			? existingDomainEvents
			: new Queue<IDomainEvent>();

		// add the domain events to the end of the queue
		domainEvents.ForEach(domainEventsQueue.Enqueue);

		// store the queue in the http context
		httpContextAccessor.HttpContext!.Items["DomainEventsQueue"] = domainEventsQueue;
	}
	
}
