namespace PharmaPOS.MVC.Models;

public class PurchaseViewModel : OrderDetailsModel<PurchaseBatchViewModel>
{
    public string SupplierName { get; init; } = string.Empty;
    public string? SupplierEmail { get; init; }
    public string? SupplierPhone { get; init; }
    
}

public class PurchaseBatchViewModel : ItemViewModel
{
    public string? Pqty { get; init; }
    public decimal PurchasePrice { get; init; }
}
