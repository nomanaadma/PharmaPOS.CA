namespace PharmaPOS_MVC.Domain.Entities;

public class MedicineBill
{
	private readonly List<BillItem> _items = [];

	// EF Core parameterless constructor
	private MedicineBill() { }

	public MedicineBill(int customerId, string name, string? email, string? mobile, int methodId, DateTime billDate, decimal subtotal, decimal tax, decimal discountValue, decimal amount, string? note)
	{
		Update(customerId, name, email, mobile, methodId, billDate, subtotal, tax, discountValue, amount, note);
		CreatedDate = DateTime.UtcNow;
	}

	public int Id { get; private set; }
	public int CustomerId { get; private set; }
	public Customer Customer { get; private set; } = null!;
	public string Name { get; private set; } = null!;
	public string? Email { get; private set; }
	public string? Mobile { get; private set; }
	public int MethodId { get; private set; }
	public PaymentMethod PaymentMethod { get; private set; } = null!;
	public DateTime BillDate { get; private set; }
	public decimal Subtotal { get; private set; }
	public decimal Tax { get; private set; }
	public decimal DiscountValue { get; private set; }
	public decimal Amount { get; private set; }
	public string? Note { get; private set; }
	public DateTime CreatedDate { get; private set; }

	public IReadOnlyCollection<BillItem> Items => _items.AsReadOnly();

	public void Update(int customerId, string name, string? email, string? mobile, int methodId, DateTime billDate, decimal subtotal, decimal tax, decimal discountValue, decimal amount, string? note)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Customer Name is required.", nameof(name));
		if (customerId <= 0)
			throw new ArgumentException("Valid Customer is required.", nameof(customerId));
		if (methodId <= 0)
			throw new ArgumentException("Valid Payment Method is required.", nameof(methodId));
		if (subtotal < 0)
			throw new ArgumentException("Subtotal cannot be negative.", nameof(subtotal));
		if (tax < 0)
			throw new ArgumentException("Tax cannot be negative.", nameof(tax));
		if (discountValue < 0)
			throw new ArgumentException("Discount value cannot be negative.", nameof(discountValue));
		if (amount < 0)
			throw new ArgumentException("Amount cannot be negative.", nameof(amount));

		CustomerId = customerId;
		Name = name;
		Email = email;
		Mobile = mobile;
		MethodId = methodId;
		BillDate = billDate;
		Subtotal = subtotal;
		Tax = tax;
		DiscountValue = discountValue;
		Amount = amount;
		Note = note;
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
