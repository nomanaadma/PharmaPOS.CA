using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Data;
using PharmaPOS.Domain.Enums;
using PharmaPOS_MVC.Extensions;
using PharmaPOS_MVC.Mappers;
using PharmaPOS_MVC.DTOs;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
public class ReportsController(ApplicationDbContext context) : BaseController
{
    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public IActionResult Index(string? name)
    {
        return View();
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> Inventory(CancellationToken ct = default)
    {
        var currentMonth = DateTime.UtcNow.ExpiryFormat();
        
		var inventoryDto = await context.Medicines
	        // 1. Order the base query first
	        .OrderByDescending(m => m.Id)
    
	        // 2. Use an intermediate projection to calculate aggregates once (replacing 'let')
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

		var inventory = ReportMapper.MapToInventoryReport(inventoryDto);

        return View(inventory);
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> IncomeExpense(DateTime? start, DateTime? end, int? medicineId, string? medicineName, CancellationToken ct = default)
    {
        
        if (start.HasValue && end.HasValue)
        {
	        // .Date strips any existing time and sets it to 00:00:00
	        start = start.Value.Date; 
    
	        // Set to 23:59:59 of the selected end date
	        end = end.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }
        else
        {
	        // Default: 1 month ago at 00:00:00
	        start = DateTime.Today.AddMonths(-1); 
    
	        // Default: Today at 23:59:59
	        end = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59); 
        }
        
        
        // 2. Get Sales Stats
        var medicineStats = await context.BillItems
	        .IdOrNullWhere(medicineId ?? 0)
	        .Where(bi => 
		        bi.Bill.BillDate >= start.Value && 
		        bi.Bill.BillDate <= end.Value)
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
	        
	        // 1. Get Batch Totals
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
	        
	        // Safely extract values (default to 0 if null)
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
        
        // Safely extract values (default to 0 if null)
        var salesAmount = medicineStats?.SalesAmount ?? 0m;
        var taxAmount = medicineStats?.TaxAmount ?? 0m;
        var discountAmount = medicineStats?.DiscountAmount ?? 0m;

        var stats = new IncomeExpenseReportViewModel
        {
	        SalesAmount = salesAmount,
	        PurchaseAmount = purchaseAmount,
	        Profit = salesAmount - purchaseAmount,
	        Quantity = salesQty,
	        TaxAmount = taxAmount,
	        DiscountAmount = discountAmount,
	        StartDate = start.Value,
	        EndDate = end.Value,
	        MedicineId = medicineId,
	        MedicineName = medicineName
        };

        return View(stats);
    }
}
