namespace PharmaPOS_MVC.Models;

public class BillingViewModel
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Mobile { get; init; }
    public DateTime BillDate { get; init; }
    public string? PaymentMethodName { get; init; }
    public decimal Subtotal { get; init; }
    public decimal DiscountValue { get; init; }
    public decimal Tax { get; init; }
    public decimal Amount { get; init; }
    public string? Note { get; init; }
    
    public List<BillingItemViewModel> Items { get; init; } = [];
}

public class BillingItemViewModel
{
    public string MedicineName { get; init; } = string.Empty;
    public string BatchName { get; init; } = string.Empty;
    public string Expiry { get; init; } = string.Empty;
    public decimal Qty { get; init; }
    public decimal SalePrice { get; init; }
    public decimal Gross { get; init; }
    public decimal DiscountValue { get; init; }
    public decimal TaxPrice { get; init; }
    public decimal Price { get; init; }
}
