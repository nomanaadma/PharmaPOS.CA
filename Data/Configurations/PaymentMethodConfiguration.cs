using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaPOS_MVC.Domain.Entities;

namespace PharmaPOS_MVC.Data.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
	public void Configure(EntityTypeBuilder<PaymentMethod> builder)
	{
		builder.ToTable("payment_method");

		builder.HasKey(pm => pm.Id);

		builder.Property(pm => pm.Id)
			.HasColumnName("id");

		builder.Property(pm => pm.Name)
			.HasColumnName("name")
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(pm => pm.Status)
			.HasColumnName("status")
			.IsRequired();

		builder.Property(pm => pm.CreatedDate)
			.HasColumnName("created_date")
			.IsRequired();

		builder.HasIndex(pm => pm.Name)
			.IsUnique();
	}
}
