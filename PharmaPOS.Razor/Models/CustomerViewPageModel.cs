namespace PharmaPOS.Razor.Models;

public class CustomerViewPageModel
{
    public CustomerViewModel Customer { get; init; } = null!;
    public IEnumerable<CustomerBillViewModel> RecentBills { get; init; } = [];
}

public class CustomerBillViewModel : Model
{
    public decimal Amount { get; init; }
    public DateTime BillDate { get; init; }
}
