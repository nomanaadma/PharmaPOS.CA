using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaPOS_MVC.Domain.Entities;

namespace PharmaPOS_MVC.Data.Configurations;

public class BillItemConfiguration : IEntityTypeConfiguration<BillItem>
{
	public void Configure(EntityTypeBuilder<BillItem> builder)
	{
		builder.ToTable("bill_items");

		builder.HasKey(bi => bi.Id);

		builder.Property(bi => bi.Id)
			.HasColumnName("id");

		builder.Property(bi => bi.BillId)
			.HasColumnName("bill_id")
			.IsRequired();

		builder.Property(bi => bi.MedicineId)
			.HasColumnName("medicine_id")
			.IsRequired();

		builder.Property(bi => bi.BatchId)
			.HasColumnName("batch")
			.IsRequired();

		builder.Property(bi => bi.Qty)
			.HasColumnName("qty")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(bi => bi.SalePrice)
			.HasColumnName("saleprice")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(bi => bi.Gross)
			.HasColumnName("gross")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(bi => bi.DiscountType)
			.HasColumnName("discounttype")
			.IsRequired();

		builder.Property(bi => bi.Discount)
			.HasColumnName("discount")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(bi => bi.DiscountValue)
			.HasColumnName("discountvalue")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(bi => bi.Tax)
			.HasColumnName("tax")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(bi => bi.TaxPrice)
			.HasColumnName("taxprice")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.Property(bi => bi.Price)
			.HasColumnName("price")
			.HasColumnType("decimal(10, 2)")
			.IsRequired();

		builder.HasOne(bi => bi.Bill)
			.WithMany(mb => mb.Items)
			.HasForeignKey(bi => bi.BillId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(bi => bi.Medicine)
			.WithMany()
			.HasForeignKey(bi => bi.MedicineId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(bi => bi.Batch)
			.WithMany()
			.HasForeignKey(bi => bi.BatchId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
