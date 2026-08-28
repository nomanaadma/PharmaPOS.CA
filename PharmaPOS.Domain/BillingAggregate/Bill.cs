using ErrorOr;
using PharmaPOS.Domain.Common;
using PharmaPOS.Domain.CustomerAggregate;
using PharmaPOS.Domain.PaymentMethodAggregate;

namespace PharmaPOS.Domain.BillingAggregate;

public class Bill : OrderDetails
{
	private readonly List<BillItem> _items = [];

	// EF Core parameterless constructor
	private Bill() { }

	private Bill(BillDetails details)
	{
		SetDetails(details);
	}

	public int CustomerId { get; private set; }
	public Customer Customer { get; private set; } = null!;
	public string Name { get; private set; } = null!;
	public string? Email { get; private set; }
	public string? Mobile { get; private set; }
	public int MethodId { get; private set; }
	public PaymentMethod PaymentMethod { get; private set; } = null!;

	public IReadOnlyCollection<BillItem> Items => _items.AsReadOnly();

	/// <summary>
	/// Factory method: validates inputs and returns an ErrorOr&lt;Bill&gt;.
	/// </summary>
	public static ErrorOr<Bill> Create(BillDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		return new Bill(details);
	}

	/// <summary>
	/// Updates the bill details. Returns ErrorOr&lt;Updated&gt; with validation.
	/// </summary>
	public ErrorOr<Updated> Update(BillDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		SetDetails(details);
		return Result.Updated;
	}

	public void AddItem(BillItem item)
	{
		_items.Add(item);
	}

	public void ClearItems()
	{
		_items.Clear();
	}

	private void SetDetails(BillDetails details)
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

	private static List<Error> ValidateDetails(BillDetails details)
	{
		var errors = new List<Error>();

		if (string.IsNullOrWhiteSpace(details.Name))
			errors.Add(BillErrors.NameRequired);

		if (details.CustomerId <= 0)
			errors.Add(BillErrors.InvalidCustomer);

		if (details.MethodId <= 0)
			errors.Add(BillErrors.InvalidPaymentMethod);
		
		if (details.Amount < 0)
			errors.Add(BillErrors.InvalidAmount);
		
		if (details.SubTotal < 0)
			errors.Add(BillErrors.InvalidSubTotal);
		
		if (details.Tax < 0)
			errors.Add(BillErrors.InvalidTax);
		
		if (details.Discount < 0)
			errors.Add(BillErrors.InvalidDiscount);
		
		return errors;
	}
}
