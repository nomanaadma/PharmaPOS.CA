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
}