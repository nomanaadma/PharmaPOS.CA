using PharmaPOS.Contracts.Responses;
using PharmaPOS.Razor.Models;

namespace PharmaPOS.Razor.Mappers;

public static class ReportMapper
{
    public static IEnumerable<InventoryReportViewModel> MapToInventoryReport(IEnumerable<InventoryReportResponse> dtos)
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


    public static IncomeExpenseReportViewModel MapToStatsReport(StatsReportResponse stats)
    {

	    var profit = stats.SalesAmount - stats.PurchaseAmount;
	    var sales = stats.SalesAmount;
	    var purchase = stats.PurchaseAmount;
	    
	    if (sales == 0 && profit < 0 && Math.Abs(profit) == purchase)
	    {
		    profit = 0;
	    }
	    
	    return new IncomeExpenseReportViewModel
	    {
		    SalesAmount = sales,
		    PurchaseAmount = purchase,
		    Profit = profit,
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
