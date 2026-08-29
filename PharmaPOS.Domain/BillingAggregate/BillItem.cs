using ErrorOr;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.Common;
using PharmaPOS.Domain.Common.ValueObjects;
using PharmaPOS.Domain.MedicineAggregate;
using PharmaPOS.Domain.PurchaseAggregate;

namespace PharmaPOS.Domain.BillingAggregate;

public class BillItem : Entity
{
	// EF Core parameterless constructor
	private BillItem() { }

	private BillItem(BillItemDetails details)
	{
		MedicineId = details.MedicineId;
		BatchId = details.BatchId;
		Pricing = details.Pricing;
	}

	public int BillId { get; private set; }
	public Bill Bill { get; private set; } = null!;
	public int MedicineId { get; private set; }
	public Medicine Medicine { get; private set; } = null!;
	public int BatchId { get; private set; }
	public Batch Batch { get; private set; } = null!;
	public PricingDetails Pricing { get; private set; } = null!;
	
	/// <summary>
	/// Factory method: validates inputs and returns an ErrorOr&lt;Bill&gt;.
	/// </summary>
	public static ErrorOr<BillItem> Create(BillItemDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		return new BillItem(details);
	}
	
	private static List<Error> ValidateDetails(BillItemDetails details)
	{
		var errors = new List<Error>();
		
		if (details.MedicineId <= 0)
			errors.Add(BillErrors.InvalidMedicine);
		
		if (details.BatchId <= 0)
			errors.Add(BillErrors.InvalidBatch);
		
		return errors;
	}
}
