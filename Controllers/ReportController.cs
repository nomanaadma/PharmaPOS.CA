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
        if (name == "inventory")
            return RedirectToAction(nameof(Inventory));
        
        if (name == "income_expenses")
            return RedirectToAction(nameof(IncomeExpense));

        ViewData["page_title"] = "Reports";
        return View();
    }

    [HttpGet]
    [Route("/Reports/Inventory")]
    public async Task<IActionResult> Inventory()
    {
        var currentMonth = DateTime.UtcNow.ToString("yyyyMM");
        
        var inventory = await context.Medicines
            .Select(m => new InventoryReportViewModel
            {
                MedicineId = m.Id,
                MedicineName = m.Name,
                TotalPurchase = m.Batches.Sum(b => b.PurchasePrice * b.Qty),
                TotalSales = m.Batches.Sum(b => b.SalePrice * b.Sold),
                Profit = m.Batches.Sum(b => b.SalePrice * b.Sold) - m.Batches.Sum(b => b.PurchasePrice * b.Sold),
                ReorderLevel = m.ReOrderLevel ?? 0,
                MinLevel = m.MinLevel ?? 0,
                LiveStock = m.Batches
#pragma warning disable CA1845
                    .Where(b => b.Status == Status.Active && string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) > 0)
#pragma warning restore CA1845
                    .Sum(b => (decimal?)(b.Qty - b.Sold)) ?? 0m
            })
            .OrderByDescending(m => m.MedicineId)
            .ToListAsync();

        ViewData["page_title"] = "Inventory Report";
        return View(inventory);
    }

    [HttpGet]
    [Route("/Reports/IncomeExpense")]
    public async Task<IActionResult> IncomeExpense(DateTime? start, DateTime? end, int? medicine_id, string? medicine_name)
    {
        if (!start.HasValue || !end.HasValue)
        {
            start = DateTime.UtcNow.Date.AddMonths(-1);
            end = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);
        }

        IncomeExpenseReportViewModel stats;

        if (medicine_id.HasValue && medicine_id.Value > 0)
        {
            var medicineStats = await context.BillItems
                .Where(bi => bi.MedicineId == medicine_id.Value && bi.Bill.BillDate >= start.Value && bi.Bill.BillDate <= end.Value)
                .GroupBy(bi => 1)
                .Select(g => new
                {
                    SalesAmount = g.Sum(bi => bi.Price),
                    PurchaseAmount = g.Sum(bi => bi.Qty * bi.Batch.PurchasePrice),
                    Quantity = g.Sum(bi => (decimal?)bi.Qty),
                    TaxAmount = g.Sum(bi => bi.TaxPrice),
                    DiscountAmount = g.Sum(bi => bi.DiscountValue)
                })
                .FirstOrDefaultAsync();

            stats = new IncomeExpenseReportViewModel
            {
                SalesAmount = medicineStats?.SalesAmount ?? 0,
                PurchaseAmount = medicineStats?.PurchaseAmount ?? 0,
                Profit = (medicineStats?.SalesAmount ?? 0) - (medicineStats?.PurchaseAmount ?? 0),
                Quantity = medicineStats?.Quantity ?? 0,
                TaxAmount = medicineStats?.TaxAmount ?? 0,
                DiscountAmount = medicineStats?.DiscountAmount ?? 0
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
        stats.MedicineId = medicine_id;
        stats.MedicineName = medicine_name;

        ViewData["page_title"] = "Stats Report";
        return View(stats);
    }
}
