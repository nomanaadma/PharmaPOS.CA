namespace PharmaPOS.MVC.Models;

public class InventoryReportViewModel
{
    public int MedicineId { get; init; }
    public string MedicineName { get; init; } = null!;
    public decimal TotalPurchase { get; init; }
    public decimal TotalSales { get; init; }
    public decimal Profit { get; init; }
    public decimal? ReorderLevel { get; init; }
    public decimal? MinLevel { get; init; }
    public decimal LiveStock { get; init; }
}

public class IncomeExpenseReportViewModel
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int? MedicineId { get; init; }
    public string? MedicineName { get; init; }

    public decimal SalesAmount { get; init; }
    public decimal PurchaseAmount { get; init; }
    public decimal Profit { get; init; }
    public decimal? Quantity { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal DiscountAmount { get; init; }
}
