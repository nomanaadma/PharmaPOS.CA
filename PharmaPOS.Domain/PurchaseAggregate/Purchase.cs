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

		DomainValidator<Purchase>.ValidateIds(errors,
			(details.SupplierId, nameof(details.SupplierId))
		);

		DomainValidator<Purchase>.ValidateNonNegativeNumbers(errors,
			(details.Amount, nameof(details.Amount)),
			(details.SubTotal, nameof(details.SubTotal)),
			(details.Tax, nameof(details.Tax)),
			(details.Discount, nameof(details.Discount))
		);
		
		return errors;
	}
}
