using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaPOS.Domain.MedicineCategoryAggregate;

namespace PharmaPOS.Application.Data.Configurations;

public class MedicineCategoryConfiguration : IEntityTypeConfiguration<MedicineCategory>
{
	public void Configure(EntityTypeBuilder<MedicineCategory> builder)
	{
		builder.ToTable("medicine_categories");

		builder.HasKey(c => c.Id);

		builder.Property(c => c.Id)
			.HasColumnName("id");

		builder.Property(c => c.Name)
			.HasColumnName("name")
			.IsRequired()
			.HasMaxLength(100);

		builder.HasIndex(c => c.Name)
			.IsUnique();
	}
}
