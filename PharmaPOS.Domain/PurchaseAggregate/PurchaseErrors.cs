using ErrorOr;

namespace PharmaPOS.Domain.PurchaseAggregate;

public static class PurchaseErrors
{
    public static Error NotFound(int id) =>
        Error.NotFound("Purchase.NotFound", $"Purchase {id} does not exist.");

    public static Error CannotDeleteSoldBatch(string batchName) =>
        Error.Conflict("Purchase.CannotDeleteSoldBatch", $"Cannot delete batch '{batchName}' because it has already been sold.");

    public static Error CannotUpdateSoldQuantity(string batchName, decimal newQty, decimal soldQty) =>
        Error.Conflict("Purchase.CannotUpdateSoldQuantity", $"Cannot update batch '{batchName}'. New quantity ({newQty}) is less than the amount already sold ({soldQty}).");
        
    public static Error CannotDeletePurchaseWithSoldBatches =>
        Error.Conflict("Purchase.CannotDeletePurchaseWithSoldBatches", "Cannot delete purchase because one or more batches have already been sold.");
    
    public static Error MissingItems =>
	    Error.Validation("Purchase.MissingItems", "Purchase must have at least one item.");

    public static Error SupplierRequired =>
	    Error.Validation("Purchase.SupplierRequired", "A valid supplier is required.");
        
    public static Error InvalidAmount =>
	    Error.Validation("Purchase.InvalidAmount", "Amount cannot be negative.");
    
    public static Error InvalidSubTotal =>
	    Error.Validation("Purchase.InvalidSubTotal", "SubTotal cannot be negative.");
    
    public static Error InvalidTax =>
	    Error.Validation("Purchase.InvalidTax", "Tax cannot be negative.");
    
    public static Error InvalidDiscount =>
	    Error.Validation("Purchase.InvalidDiscount", "Discount cannot be negative.");
    
}
