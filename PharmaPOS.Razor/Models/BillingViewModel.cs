namespace PharmaPOS.Razor.Models;

public class BillingViewModel : OrderDetailsModel<BillingItemViewModel>
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Mobile { get; init; }
    public string? PaymentMethodName { get; init; }
}

public class BillingItemViewModel : ItemViewModel;