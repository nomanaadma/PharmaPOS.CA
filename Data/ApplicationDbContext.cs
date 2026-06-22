using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Domain.Entities;

namespace PharmaPOS_MVC.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
	public DbSet<Setting> Settings { get; set; } = null!;
	public DbSet<Media> Media { get; set; } = null!;
	public DbSet<Supplier> Suppliers { get; set; } = null!;
	public DbSet<MedicineCategory> MedicineCategories { get; set; } = null!;
	public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
	public DbSet<Customer> Customers { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		
		builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		
	}
}