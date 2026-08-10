using PharmaPOS_MVC.DTOs;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Mappers;

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

}
