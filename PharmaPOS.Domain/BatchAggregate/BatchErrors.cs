using ErrorOr;
namespace PharmaPOS.Domain.BatchAggregate;

public static class BatchErrors
{
	public static Error CannotDeleteSoldBatch(string batchName) =>
		Error.Conflict("Batch.CannotDeleteSoldBatch", $"Cannot delete batch '{batchName}' because it has already been sold.");

	public static Error CannotUpdateSoldQuantity(string batchName, decimal newQty, decimal soldQty) =>
		Error.Conflict("Batch.CannotUpdateSoldQuantity", $"Cannot update batch '{batchName}'. New quantity ({newQty}) is less than the amount already sold ({soldQty}).");
	
	public static Error CannotDeletePurchaseWithSoldBatches =>
		Error.Conflict("Batch.CannotDeletePurchaseWithSoldBatches", "Cannot delete purchase because one or more batches have already been sold.");
	
	public static Error InvalidSaleQuantity => 
		Error.Validation(
			code: "Batch.InvalidSaleQuantity", 
			description: "Sale quantity must be greater than zero.");

	public static Error InsufficientStock => 
		Error.Validation(
			code: "Batch.InsufficientStock", 
			description: "There is not enough stock in this batch to complete the sale.");
	
}