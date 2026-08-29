using ErrorOr;
namespace PharmaPOS.Domain.BatchAggregate;

public static class BatchErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("Batch.NotFound", $"Batch {id} does not exist.");

	public static Error BatchNameRequired =>
		Error.Validation("Batch.BatchNameRequired", "Batch name is required.");

	public static Error MedicineRequired =>
		Error.Validation("Batch.MedicineRequired", "Medicine is required.");
	
	public static Error PurchaseRequired =>
		Error.Validation("Batch.PurchaseRequired", "Purchase is required.");

	public static Error ExpiryRequired =>
		Error.Validation("Batch.ExpiryRequired", "Expiry is required.");

	public static Error InvalidPurchasePrice =>
		Error.Validation("Batch.InvalidPurchasePrice", "Purchase Price cannot be negative.");

	public static Error CannotDeleteSoldBatch(string batchName) =>
		Error.Conflict("Batch.CannotDeleteSoldBatch", $"Cannot delete batch '{batchName}' because it has already been sold.");

	public static Error CannotUpdateSoldQuantity(string batchName, decimal newQty, decimal soldQty) =>
		Error.Conflict("Batch.CannotUpdateSoldQuantity", $"Cannot update batch '{batchName}'. New quantity ({newQty}) is less than the amount already sold ({soldQty}).");
	
	public static Error CannotDeletePurchaseWithSoldBatches =>
		Error.Conflict("Batch.CannotDeletePurchaseWithSoldBatches", "Cannot delete purchase because one or more batches have already been sold.");
	
	
}