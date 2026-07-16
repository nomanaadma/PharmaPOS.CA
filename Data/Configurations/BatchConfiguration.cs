using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Domain.Enums;

namespace PharmaPOS_MVC.Data.Configurations;

public class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
	public void Configure(EntityTypeBuilder<Batch> builder)
	{
		builder.ToTable("medicine_batch");

		builder.HasKey(mb => mb.Id);

		builder.Property(mb => mb.Id)
			.HasColumnName("id");

		builder.Property(mb => mb.BatchName)
			.HasColumnName("batch")
			.IsRequired()
			.HasMaxLength(255);

		builder.Property(mb => mb.Expiry)
			.HasColumnName("expiry")
			.IsRequired()
			.HasMaxLength(10);

		builder.Property(mb => mb.Pqty)
			.HasColumnName("pqty")
			.HasMaxLength(10);

		builder.Property(mb => mb.Qty)
			.HasColumnName("qty")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(mb => mb.SalePrice)
			.HasColumnName("saleprice")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(mb => mb.PurchasePrice)
			.HasColumnName("purchaseprice")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(mb => mb.Gross)
			.HasColumnName("gross")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(mb => mb.DiscountType)
			.HasColumnName("discounttype")
			.IsRequired();

		builder.Property(mb => mb.Discount)
			.HasColumnName("discount")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(mb => mb.DiscountValue)
			.HasColumnName("discountvalue")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(mb => mb.Tax)
			.HasColumnName("tax")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(mb => mb.TaxPrice)
			.HasColumnName("taxprice")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(mb => mb.Price)
			.HasColumnName("price")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(mb => mb.Sold)
			.HasColumnName("sold")
			.HasColumnType("decimal(10, 2)")
			.HasDefaultValue(0.00)
			.IsRequired();

		builder.Property(mb => mb.MedicineId)
			.HasColumnName("medicine_id")
			.IsRequired();

		builder.Property(mb => mb.PurchaseId)
			.HasColumnName("purchase_id")
			.IsRequired();

		builder.Property(mb => mb.Status)
			.HasColumnName("status")
			.HasColumnType("boolean")
			.HasConversion(
				v => v == Status.Active,
				v => v ? Status.Active : Status.InActive)
			.IsRequired();

		builder.Property(mb => mb.CreatedDate)
			.HasColumnName("created_date")
			.IsRequired();

		builder.HasOne(mb => mb.Medicine)
			.WithMany()
			.HasForeignKey(mb => mb.MedicineId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(mb => mb.Purchase)
			.WithMany(mp => mp.Batches)
			.HasForeignKey(mb => mb.PurchaseId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
