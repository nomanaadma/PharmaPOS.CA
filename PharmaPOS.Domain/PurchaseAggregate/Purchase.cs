using ErrorOr;
using PharmaPOS.Domain.Common;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Domain.PurchaseAggregate;

public class Purchase : OrderDetails
{
	// EF Core parameterless constructor
	private Purchase() { }

	private Purchase(PurchaseDetails details)
	{
		SetDetails(details);
	}

	public int SupplierId { get; private set; }
	public Supplier Supplier { get; private set; } = null!;
	
	/// <summary>
	/// Factory method: validates inputs and returns an ErrorOr&lt;Purchase&gt;.
	/// </summary>
	public static ErrorOr<Purchase> Create(PurchaseDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		return new Purchase(details);
	}

	/// <summary>
	/// Updates the purchase details. Returns ErrorOr&lt;Updated&gt; with validation.
	/// </summary>
	public ErrorOr<Updated> Update(PurchaseDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		SetDetails(details);
		return Result.Updated;
	}
	
	private void SetDetails(PurchaseDetails details)
	{
		SupplierId = details.SupplierId;
		Date = details.Date;
		SubTotal = details.SubTotal;
		Tax = details.Tax;
		Discount = details.Discount;
		Amount = details.Amount;
		Note = details.Note;
	}

	private static List<Error> ValidateDetails(PurchaseDetails details)
	{
		var errors = new List<Error>();

		if (details.SupplierId <= 0)
			errors.Add(PurchaseErrors.SupplierRequired);

		if (details.Amount < 0)
			errors.Add(PurchaseErrors.InvalidAmount);
		
		if (details.SubTotal < 0)
			errors.Add(PurchaseErrors.InvalidSubTotal);
		
		if (details.Tax < 0)
			errors.Add(PurchaseErrors.InvalidTax);
		
		if (details.Discount < 0)
			errors.Add(PurchaseErrors.InvalidDiscount);
		
		return errors;
	}
}
