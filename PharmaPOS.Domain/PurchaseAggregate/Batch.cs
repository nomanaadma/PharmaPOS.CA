using ErrorOr;
using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.Common.ValueObjects;
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Domain.PurchaseAggregate;

public class Batch : EntityWithDate
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
	public Medicine Medicine { get; private set; } = null!;
	public int PurchaseId { get; private set; }
	public Purchase Purchase { get; private set; } = null!;
	public Status Status { get; private set; } = Status.Active;

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
	}

	private static List<Error> ValidateDetails(BatchDetails details)
	{
		var errors = new List<Error>();

		if (string.IsNullOrWhiteSpace(details.BatchName))
			errors.Add(Error.Validation("Batch.BatchNameRequired", "Batch name is required."));

		if (details.MedicineId <= 0)
			errors.Add(Error.Validation("Batch.MedicineRequired", "Medicine is required."));
		
		if (string.IsNullOrWhiteSpace(details.Expiry))
			errors.Add(Error.Validation("Batch.Expiry", "Expiry is required."));
		
		if (details.PurchasePrice < 0)
			errors.Add(Error.Validation("Batch.PurchasePrice", "Purchase Price cannot be negative."));
		
		return errors;
	}
}
