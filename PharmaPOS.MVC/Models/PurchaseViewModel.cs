namespace PharmaPOS.MVC.Models;

public class PurchaseViewModel
{
    public int Id { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public string? SupplierEmail { get; init; }
    public string? SupplierPhone { get; init; }
    public DateTime Date { get; init; }
    public decimal SubTotal { get; init; }
    public decimal Tax { get; init; }
    public decimal DiscountValue { get; init; }
    public decimal Amount { get; init; }
    public string? Note { get; init; }
    
    public List<PurchaseBatchViewModel> Batches { get; init; } = [];
}

public class PurchaseBatchViewModel
{
    public string MedicineName { get; init; } = string.Empty;
    public string BatchName { get; init; } = string.Empty;
    public string Expiry { get; init; } = string.Empty;
    public string? Pqty { get; init; }
    public decimal Qty { get; init; }
    public decimal PurchasePrice { get; init; }
    public decimal SalePrice { get; init; }
    public decimal Gross { get; init; }
    public decimal DiscountValue { get; init; }
    public decimal TaxPrice { get; init; }
    public decimal Price { get; init; }
}
