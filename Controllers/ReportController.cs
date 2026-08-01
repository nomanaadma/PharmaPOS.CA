using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Enums;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Reports/[action]")]
public class ReportController(ApplicationDbContext context) : Controller
{
    [HttpGet("/Reports")]
    public IActionResult Index(string? name)
    {
        return View();
    }

    [HttpGet("/Reports/Inventory")]
    public async Task<IActionResult> Inventory()
    {
        var currentMonth = DateTime.UtcNow.ToString("yyyyMM");
        
        var inventory = await context.Medicines
	        // 1. Order the base query first
	        .OrderByDescending(m => m.Id)
    
	        // 2. Use an intermediate projection to calculate aggregates once (replacing 'let')
	        .Select(m => new
	        {
		        Medicine = m,
        
		        TotalPurchase = m.Batches
			        .Where(b => b.Qty > 0)
			        .Sum(b => (decimal?)((b.Price / b.Qty) * b.Sold)) ?? 0,
            
		        TotalSales = context.BillItems
			        .Where(bi => bi.MedicineId == m.Id)
			        .Sum(bi => (decimal?)bi.Price) ?? 0,
            
		        LiveStock = m.Batches
			        .Where(b => b.Status == Status.Active && 
			                    string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
			        .Sum(b => (decimal?)(b.Qty - b.Sold)) ?? 0
	        })
    
	        // 3. Project into your final ViewModel, reusing the calculated variables
	        .Select(x => new InventoryReportViewModel
	        {
		        MedicineId = x.Medicine.Id,
		        MedicineName = x.Medicine.Name,
		        TotalPurchase = x.TotalPurchase,
		        TotalSales = x.TotalSales,
        
		        // No redundant SQL queries! Just basic math on the properties we already resolved.
		        Profit = x.TotalSales - x.TotalPurchase, 
        
		        ReorderLevel = x.Medicine.ReOrderLevel,
		        MinLevel = x.Medicine.MinLevel,
		        LiveStock = x.LiveStock
	        })
	        .ToListAsync();

        return View(inventory);
    }

    [HttpGet("/Reports/IncomeExpense")]
    public async Task<IActionResult> IncomeExpense(DateTime? start, DateTime? end, int? medicineId, string? medicineName)
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
        
        IncomeExpenseReportViewModel stats;

        if (medicineId is > 0)
        {
	        
	        // 1. Get Batch Totals
	        var batchTotals = await context.Batches
		        .Where(b => b.Status == Status.Active && b.MedicineId == medicineId)
		        .GroupBy(b => 1) 
		        .Select(g => new 
		        {
			        TotalQty = g.Sum(b => (decimal?)b.Qty) ?? 0m,
			        TotalPrice = g.Sum(b => (decimal?)b.Price) ?? 0m,
			        TotalSold = g.Sum(b => (decimal?)b.Sold) ?? 0m
		        })
		        .FirstOrDefaultAsync();
	        
	        // Safely extract values (default to 0 if null)
	        var totalQty = batchTotals?.TotalQty ?? 0m;
	        var totalPrice = batchTotals?.TotalPrice ?? 0m;
	        var totalSold = batchTotals?.TotalSold ?? 0m;
	        
	        var purchaseAmount = totalQty > 0 ? (totalPrice / totalQty) * totalSold : 0m;
	        
	        // 2. Get Sales Stats
	        var medicineStats = await context.BillItems
		        .Where(bi => 
			        bi.MedicineId == medicineId.Value && 
			        bi.Bill.BillDate >= start.Value && 
			        bi.Bill.BillDate <= end.Value)
		        .GroupBy(bi => 1)
		        .Select(g => new
		        {
			        SalesAmount = g.Sum(bi => (decimal?)bi.Price) ?? 0m,
			        Quantity = g.Sum(bi => (decimal?)bi.Qty) ?? 0m,
			        TaxAmount = g.Sum(bi => (decimal?)bi.TaxPrice) ?? 0m,
			        DiscountAmount = g.Sum(bi => (decimal?)bi.DiscountValue) ?? 0m
		        })
		        .FirstOrDefaultAsync();

	        // Safely extract values (default to 0 if null)
	        var salesAmount = medicineStats?.SalesAmount ?? 0m;
	        var salesQty = medicineStats?.Quantity ?? 0m;
	        var taxAmount = medicineStats?.TaxAmount ?? 0m;
	        var discountAmount = medicineStats?.DiscountAmount ?? 0m;


	        stats = new IncomeExpenseReportViewModel
	        {
		        SalesAmount = salesAmount,
		        PurchaseAmount = purchaseAmount,
		        Profit = salesAmount - purchaseAmount,
		        Quantity = salesQty,
		        TaxAmount = taxAmount,
		        DiscountAmount = discountAmount
	        };
        }
        else
        {
            var salesStats = await context.Bills
                .Where(b => b.BillDate >= start.Value && b.BillDate <= end.Value)
                .GroupBy(b => 1)
                .Select(g => new
                {
                    SalesAmount = g.Sum(b => b.Amount),
                    TaxAmount = g.Sum(b => b.Tax),
                    DiscountAmount = g.Sum(b => b.DiscountValue)
                })
                .FirstOrDefaultAsync();

            var purchaseStats = await context.Purchases
                .Where(p => p.Date >= start.Value && p.Date <= end.Value)
                .GroupBy(p => 1)
                .Select(g => new
                {
                    PurchaseAmount = g.Sum(p => p.Amount)
                })
                .FirstOrDefaultAsync();

            stats = new IncomeExpenseReportViewModel
            {
                SalesAmount = salesStats?.SalesAmount ?? 0,
                PurchaseAmount = purchaseStats?.PurchaseAmount ?? 0,
                Profit = (salesStats?.SalesAmount ?? 0) - (purchaseStats?.PurchaseAmount ?? 0),
                TaxAmount = salesStats?.TaxAmount ?? 0,
                DiscountAmount = salesStats?.DiscountAmount ?? 0,
                Quantity = null
            };
        }

        stats.StartDate = start.Value;
        stats.EndDate = end.Value;
        stats.MedicineId = medicineId;
        stats.MedicineName = medicineName;

        return View(stats);
    }
}
