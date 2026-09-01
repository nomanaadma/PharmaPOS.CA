using ErrorOr;

namespace PharmaPOS.Domain.BillingAggregate;

public static class BillErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Bill.NotFound", $"Bill {id} does not exist.");

	public static Error BatchNotFound(int batchId, int medicineId) =>
		Error.NotFound("Bill.BatchNotFound", $"Batch {batchId} not found for Medicine {medicineId}.");

	public static Error CustomerNameRequired =>
		Error.Validation("Bill.CustomerNameRequired",
			"Customer name is required when no existing customer is selected.");
	
	public static Error InsufficientStock(string batchName, decimal available, decimal requested) =>
		Error.Validation("Bill.InsufficientStock",
			$"Insufficient stock for batch {batchName}. Available: {available}, Requested: {requested}");

	public static Error MissingItems =>
		Error.Validation("Bill.MissingItems", "Bill must have at least one item.");
	
	public static Error InvalidMedicine =>
		Error.Validation("BillItem.InvalidMedicine", "Valid Medicine is required.");
    
	public static Error InvalidBatch =>
		Error.Validation("BillItem.InvalidBatch", "Valid Batch is required.");
	
}
