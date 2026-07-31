namespace PharmaPOS_MVC.Models;

public class CustomerViewPageModel
{
    public CustomerViewModel Customer { get; init; } = null!;
    public IEnumerable<CustomerBillViewModel> RecentBills { get; init; } = [];
}

public class CustomerBillViewModel
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public DateTime BillDate { get; init; }
}
