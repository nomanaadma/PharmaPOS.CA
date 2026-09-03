namespace PharmaPOS.Razor.Models;

public class CustomerBillViewModel : Model
{
	public decimal Amount { get; init; }
	public DateTime BillDate { get; init; }
}
