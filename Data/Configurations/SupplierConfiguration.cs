using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaPOS_MVC.Domain.Entities;

namespace PharmaPOS_MVC.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
	public void Configure(EntityTypeBuilder<Supplier> builder)
	{
		builder.ToTable("suppliers");

		builder.HasKey(s => s.Id);

		builder.Property(s => s.Id)
			.HasColumnName("id");

		builder.Property(s => s.Name)
			.HasColumnName("name")
			.IsRequired()
			.HasMaxLength(150);

		builder.Property(s => s.Email)
			.HasColumnName("email")
			.HasMaxLength(254);

		builder.Property(s => s.Phone)
			.HasColumnName("phone")
			.IsRequired()
			.HasMaxLength(30);

		builder.Property(s => s.Address)
			.HasColumnName("address")
			.HasMaxLength(500);

		builder.Property(s => s.CreatedDate)
			.HasColumnName("created_date")
			.IsRequired();
	}
}
