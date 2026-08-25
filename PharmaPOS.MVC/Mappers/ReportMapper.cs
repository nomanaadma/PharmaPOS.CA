using PharmaPOS.MVC.DTOs;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Mappers;

public static class ReportMapper
{
    public static IEnumerable<InventoryReportViewModel> MapToInventoryReport(IEnumerable<InventoryReportDto> dtos)
    {
        return dtos.Select(x => new InventoryReportViewModel
        {
            MedicineId = x.Medicine.Id,
            MedicineName = x.Medicine.Name,
            TotalPurchase = x.TotalPurchase,
            TotalSales = x.TotalSales,
            Profit = x.TotalSales - x.TotalPurchase,
            ReorderLevel = x.Medicine.ReOrderLevel,
            MinLevel = x.Medicine.MinLevel,
            LiveStock = x.LiveStock
        });
    }


    public static IncomeExpenseReportViewModel MapToStatsReport(StatsReportDto stats)
    {
	    return new IncomeExpenseReportViewModel
	    {
		    SalesAmount = stats.SalesAmount,
		    PurchaseAmount = stats.PurchaseAmount,
		    Profit = stats.SalesAmount - stats.PurchaseAmount,
		    Quantity = stats.Quantity,
		    TaxAmount = stats.TaxAmount,
		    DiscountAmount = stats.DiscountAmount,
		    StartDate = stats.StartDate,
		    EndDate = stats.EndDate,
		    MedicineId = stats.MedicineId,
		    MedicineName = stats.MedicineName
	    };
    }
}
