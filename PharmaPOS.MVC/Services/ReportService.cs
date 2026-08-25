using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.DTOs;
using PharmaPOS.MVC.Extensions;

namespace PharmaPOS.MVC.Services;

public class ReportService(ApplicationDbContext context)
{
	public async Task<List<InventoryReportDto>> GetInventoryReportAsync(CancellationToken ct = default)
	{
		var currentMonth = DateTime.UtcNow.ExpiryFormat();
		
		return await context.Medicines
			.OrderByDescending(m => m.Id)
			.Select(m => new InventoryReportDto
			(
				m,
				m.Batches
					.Where(b => b.Pricing.Qty > 0)
					.Sum(b => (decimal?)((b.Pricing.Price / b.Pricing.Qty) * b.Sold)) ?? 0,
			
				context.BillItems
					.Where(bi => bi.MedicineId == m.Id)
					.Sum(bi => (decimal?)bi.Pricing.Price) ?? 0,
			
				m.Batches
					.Where(b => b.Status == Status.Active && 
								string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
					.Sum(b => (decimal?)(b.Pricing.Qty - b.Sold)) ?? 0
			))
			.ToListAsync(ct);
	}

	public async Task<StatsReportDto> GetIncomeExpenseReportAsync(DateTime? start, DateTime? end, int? medicineId, string? medicineName, CancellationToken ct = default)
	{
		if (start.HasValue && end.HasValue)
		{
			start = start.Value.Date; 
			end = end.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
		}
		else
		{
			start = DateTime.Today.AddMonths(-1); 
			end = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59); 
		}
		
		var medicineStats = await context.BillItems
			.IdOrNullWhere(medicineId ?? 0)
			.Where(bi => 
				bi.Bill.Date >= start.Value && 
				bi.Bill.Date <= end.Value)
			.GroupBy(bi => 1)
			.Select(g => new
			{
				SalesAmount = g.Sum(bi => (decimal?)bi.Pricing.Price) ?? 0m,
				Quantity = g.Sum(bi => (decimal?)bi.Pricing.Qty) ?? 0m,
				TaxAmount = g.Sum(bi => (decimal?)bi.Pricing.TaxPrice) ?? 0m,
				DiscountAmount = g.Sum(bi => (decimal?)bi.Pricing.DiscountValue) ?? 0m
			})
			.FirstOrDefaultAsync(ct);

		decimal purchaseAmount;
		var salesQty = 0m; 

		if (medicineId is > 0)
		{
			var batchTotals = await context.Batches
				.Where(b => b.Status == Status.Active && b.MedicineId == medicineId)
				.GroupBy(b => 1) 
				.Select(g => new 
				{
					TotalQty = g.Sum(b => (decimal?)b.Pricing.Qty) ?? 0m,
					TotalPrice = g.Sum(b => (decimal?)b.Pricing.Price) ?? 0m,
					TotalSold = g.Sum(b => (decimal?)b.Sold) ?? 0m
				})
				.FirstOrDefaultAsync(ct);
			
			var totalQty = batchTotals?.TotalQty ?? 0m;
			var totalPrice = batchTotals?.TotalPrice ?? 0m;
			var totalSold = batchTotals?.TotalSold ?? 0m;
			
			purchaseAmount = totalQty > 0 ? (totalPrice / totalQty) * totalSold : 0m;
			salesQty = medicineStats?.Quantity ?? 0m;
		}
		else
		{
			var purchaseStats = await context.Purchases
				.Where(p => p.Date >= start.Value && p.Date <= end.Value)
				.GroupBy(p => 1)
				.Select(g => new
				{
					PurchaseAmount = g.Sum(p => p.Amount)
				})
				.FirstOrDefaultAsync(ct);

			purchaseAmount = purchaseStats?.PurchaseAmount ?? 0;
		}
		
		var salesAmount = medicineStats?.SalesAmount ?? 0m;
		var taxAmount = medicineStats?.TaxAmount ?? 0m;
		var discountAmount = medicineStats?.DiscountAmount ?? 0m;

		return new StatsReportDto
		(
			StartDate: start.Value,
			EndDate: end.Value,
			MedicineId: medicineId,
			MedicineName: medicineName,
			SalesAmount: salesAmount,
			PurchaseAmount: purchaseAmount,
			Quantity: salesQty,
			TaxAmount: taxAmount,
			DiscountAmount: discountAmount
		);
	}
}
