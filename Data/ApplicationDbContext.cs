using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Domain.Entities;

namespace PharmaPOS_MVC.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
	public DbSet<Setting> Settings { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<Setting>(entity =>
		{
			entity.ToTable("settings");
			entity.HasKey(s => s.Id);
			entity.Property(s => s.Id).HasColumnName("id");
			entity.Property(s => s.Logo).HasColumnName("logo");
			entity.Property(s => s.LogoIcon).HasColumnName("logo_icon");
			entity.Property(s => s.LgBackground).HasColumnName("lg_background");
			entity.Property(s => s.Favicon).HasColumnName("favicon");
			entity.Property(s => s.Name).HasColumnName("name").IsRequired();
			entity.Property(s => s.LegalName).HasColumnName("legal_name").IsRequired();
			entity.Property(s => s.Mail).HasColumnName("mail").IsRequired();
			entity.Property(s => s.Phone).HasColumnName("phone").IsRequired();

			entity.OwnsOne(s => s.Address, a =>
			{
				a.Property(p => p.Line1).HasColumnName("address_line1");
				a.Property(p => p.Line2).HasColumnName("address_line2");
				a.Property(p => p.City).HasColumnName("city");
				a.Property(p => p.Country).HasColumnName("country");
				a.Property(p => p.Postal).HasColumnName("postal");
			});
		});
	}
}