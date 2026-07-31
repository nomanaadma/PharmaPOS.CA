namespace PharmaPOS_MVC.Models;

public class InventoryReportViewModel
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = null!;
    public decimal TotalPurchase { get; set; }
    public decimal TotalSales { get; set; }
    public decimal Profit { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MinLevel { get; set; }
    public decimal LiveStock { get; set; }
}

public class IncomeExpenseReportViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? MedicineId { get; set; }
    public string? MedicineName { get; set; }

    public decimal SalesAmount { get; set; }
    public decimal PurchaseAmount { get; set; }
    public decimal Profit { get; set; }
    public decimal? Quantity { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
}
