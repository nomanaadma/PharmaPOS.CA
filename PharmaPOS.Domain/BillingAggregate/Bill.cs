using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.CustomerAggregate;
using PharmaPOS.Domain.PaymentMethodAggregate;

namespace PharmaPOS.Domain.BillingAggregate;

public class Bill : OrderDetails
{
	private readonly List<BillItem> _items = [];

	// EF Core parameterless constructor
	private Bill() { }

	public Bill(BillDetails details)
	{
		Update(details);
	}

	public int CustomerId { get; private set; }
	public Customer Customer { get; private set; } = null!;
	public string Name { get; private set; } = null!;
	public string? Email { get; private set; }
	public string? Mobile { get; private set; }
	public int MethodId { get; private set; }
	public PaymentMethod PaymentMethod { get; private set; } = null!;

	public IReadOnlyCollection<BillItem> Items => _items.AsReadOnly();

	public void Update(BillDetails details)
	{
		CustomerId = details.CustomerId;
		Name = details.Name;
		Email = details.Email;
		Mobile = details.Mobile;
		MethodId = details.MethodId;
		Date = details.BillDate;
		SubTotal = details.SubTotal;
		Tax = details.Tax;
		Discount = details.Discount;
		Amount = details.Amount;
		Note = details.Note;
	}

	public void AddItem(BillItem item)
	{
		_items.Add(item);
	}

	public void RemoveItem(BillItem item)
	{
		_items.Remove(item);
	}

	public void ClearItems()
	{
		_items.Clear();
	}
}
