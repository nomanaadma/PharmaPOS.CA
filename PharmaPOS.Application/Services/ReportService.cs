using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Extensions;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.MedicineAggregate;
using PharmaPOS.Domain.PurchaseAggregate;

namespace PharmaPOS.Application.Services;

public class ReportService(
	IRepository<Medicine> medicineRepo,
	IRepository<Batch> batchRepo,
	IRepository<BillItem> billItemRepo,
	IRepository<Bill> billRepo,
	IRepository<Purchase> purchaseRepo) : IApplicationService
{
	public async Task<List<InventoryReportResponse>> GetInventoryReportAsync(CancellationToken ct = default)
	{
		var currentMonth = DateTime.UtcNow.ExpiryFormat();
		
		return await medicineRepo.Query()
			.OrderByDescending(m => m.Id)
			.Select(m => new InventoryReportResponse
			(
				m,
				batchRepo.Query()
					.Where(b => b.MedicineId == m.Id)
					.Where(b => b.Pricing.Qty > 0)
					.Sum(b => (decimal?)((b.Pricing.Price / b.Pricing.Qty) * b.Sold)) ?? 0,
			
				billItemRepo.Query()
					.Where(bi => bi.MedicineId == m.Id)
					.Sum(bi => (decimal?)bi.Pricing.Price) ?? 0,
			
				batchRepo.Query()
					.Where(b => b.MedicineId == m.Id)
					.Where(b => b.Status == Status.Active && 
								string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
					.Sum(b => (decimal?)(b.Pricing.Qty - b.Sold)) ?? 0
			))
			.ToListAsync(ct);
	}

	public async Task<StatsReportResponse> GetIncomeExpenseReportAsync(DateTime? start, DateTime? end, int? medicineId, string? medicineName, CancellationToken ct = default)
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
		
		var medicineStats = await billRepo.Query()
			.Where(b => 
				b.Date >= start.Value &&
				b.Date <= end.Value)
			.SelectMany(b => b.Items)
			.Where(bi => medicineId == null || bi.MedicineId == medicineId)
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

		if (medicineId > 0)
		{
			purchaseAmount = await batchRepo.Query()
				.Where(b => b.Status == Status.Active && b.MedicineId == medicineId)
				.SumAsync(b => b.Pricing.Qty > 0 
					? (decimal?)((b.Pricing.Price / b.Pricing.Qty) * b.Sold) 
					: 0m, ct) ?? 0m;
			
			salesQty = medicineStats?.Quantity ?? 0m;
		}
		else
		{
			purchaseAmount = await purchaseRepo.Query()
				.Where(p => p.Date >= start.Value && p.Date <= end.Value)
				.Join(batchRepo.Query(),
					purchase => purchase.Id,
					batch => batch.PurchaseId,
					(purchase, batch) => batch)
				.Where(b => b.Sold > 0 && b.Pricing.Qty > 0)
				.SumAsync(b => (decimal?)((b.Pricing.Price / b.Pricing.Qty) * b.Sold), ct) ?? 0m;
		}
		
		var salesAmount = medicineStats?.SalesAmount ?? 0m;
		var taxAmount = medicineStats?.TaxAmount ?? 0m;
		var discountAmount = medicineStats?.DiscountAmount ?? 0m;

		return new StatsReportResponse
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
