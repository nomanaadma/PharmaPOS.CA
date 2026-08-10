using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Mappers;

public static class StockMapper
{
    public static StockViewModel MapToStock(Batch batch, string medicineName)
    {
        return new StockViewModel
        {
            Id = batch.Id,
            MedicineId = batch.MedicineId,
            PurchaseId = batch.PurchaseId,
            Name = medicineName,
            Batch = batch.BatchName,
            Expiry = batch.Expiry,
            PurchasePrice = batch.PurchasePrice,
            SalePrice = batch.SalePrice,
            Qty = batch.Qty,
            Sold = batch.Sold,
            Available = batch.Qty - batch.Sold
        };
    }

    public static IEnumerable<StockViewModel> MapToStocks(IEnumerable<Batch> batches, string medicineName)
    {
        return batches.Select(b => MapToStock(b, medicineName)).ToList();
    }
}
