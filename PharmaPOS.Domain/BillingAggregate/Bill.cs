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
		if (string.IsNullOrWhiteSpace(details.Name))
			throw new ArgumentException("Customer Name is required.", nameof(details.Name));
		if (details.CustomerId <= 0)
			throw new ArgumentException("Valid Customer is required.", nameof(details.CustomerId));
		if (details.MethodId <= 0)
			throw new ArgumentException("Valid Payment Method is required.", nameof(details.MethodId));
		if (details.SubTotal < 0)
			throw new ArgumentException("Subtotal cannot be negative.", nameof(details.SubTotal));
		if (details.Tax < 0)
			throw new ArgumentException("Tax cannot be negative.", nameof(details.Tax));
		if (details.Discount < 0)
			throw new ArgumentException("Discount value cannot be negative.", nameof(details.Discount));
		if (details.Amount < 0)
			throw new ArgumentException("Amount cannot be negative.", nameof(details.Amount));

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
