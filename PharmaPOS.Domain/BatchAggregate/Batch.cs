using ErrorOr;
using PharmaPOS.Domain.Common;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.Common.ValueObjects;

namespace PharmaPOS.Domain.BatchAggregate;

public class Batch : AggregateRoot
{
	// EF Core parameterless constructor
	private Batch() { }

	private Batch(BatchDetails details)
	{
		SetDetails(details);
	}

	public string BatchName { get; private set; } = null!;
	public string Expiry { get; private set; } = null!;
	public string? Pqty { get; private set; }
	public PricingDetails Pricing { get; private set; } = null!;
	public decimal PurchasePrice { get; private set; }
	public decimal Sold { get; private set; }
	public int MedicineId { get; private set; }
	public int PurchaseId { get; private set; }
	public Status Status { get; private set; } = Status.Active;
	public DateTime CreatedDate { get; private init; }


	public static ErrorOr<Batch> Create(BatchDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		return new Batch(details);
	}

	public ErrorOr<Updated> Update(BatchDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		SetDetails(details);
		return Result.Updated;
	}

	public void UpdateSold(decimal sold)
	{
		Sold = sold;
	}

	public void UpdateStatus(Status status)
	{
		Status = status;
	}

	private void SetDetails(BatchDetails details)
	{
		BatchName = details.BatchName;
		Expiry = details.Expiry;
		Pqty = details.Pqty;
		Pricing = details.Pricing;
		PurchasePrice = details.PurchasePrice;
		MedicineId = details.MedicineId;
		PurchaseId = details.PurchaseId;
	}

	private static List<Error> ValidateDetails(BatchDetails details)
	{
		var errors = new List<Error>();
		
		DomainValidator<Batch>.ValidateRequiredStrings(errors,
			(details.BatchName, nameof(details.BatchName)),
			(details.Expiry, nameof(details.Expiry))
		);

		DomainValidator<Batch>.ValidateIds(errors,
			(details.MedicineId, nameof(details.MedicineId)),
			(details.PurchaseId, nameof(details.PurchaseId))
		);

		DomainValidator<Batch>.ValidateNonNegativeNumbers(errors,
			(details.PurchasePrice, nameof(details.PurchasePrice))
		);
		
		
		return errors;
	}
}
