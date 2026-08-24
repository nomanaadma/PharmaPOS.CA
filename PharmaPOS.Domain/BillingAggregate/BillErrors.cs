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
	
	
	public static Error InvalidAmount =>
		Error.Validation("Bill.InvalidAmount", "Amount cannot be negative.");
    
	public static Error InvalidSubTotal =>
		Error.Validation("Bill.InvalidAmount", "SubTotal cannot be negative.");
    
	public static Error InvalidTax =>
		Error.Validation("Bill.InvalidAmount", "Tax cannot be negative.");
    
	public static Error InvalidDiscount =>
		Error.Validation("Bill.InvalidAmount", "Discount cannot be negative.");
	
	public static Error NameRequired =>
		Error.Validation("Bill.NameRequired", "Customer name is required.");
	
	public static Error InvalidCustomer =>
		Error.Validation("Bill.InvalidCustomer", "A valid customer is required.");
    
	public static Error InvalidPaymentMethod =>
		Error.Validation("Bill.InvalidPaymentMethod", "A valid payment method is required.");
	
	
	public static Error InvalidMedicine =>
		Error.Validation("BillItem.InvalidMedicine", "Valid Medicine is required.");
    
	public static Error InvalidBatch =>
		Error.Validation("BillItem.InvalidBatch", "Valid Batch is required.");
	
}
