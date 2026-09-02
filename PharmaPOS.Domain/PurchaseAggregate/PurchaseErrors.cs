using ErrorOr;

namespace PharmaPOS.Domain.PurchaseAggregate;

public static class PurchaseErrors
{
    public static Error NotFound(int id) =>
        Error.NotFound("Purchase.NotFound", $"Purchase {id} does not exist.");
    
    public static readonly Error MissingItems =
	    Error.Validation("Purchase.MissingItems", "Purchase must have at least one item.");
    
    public static Error SupplierNotFound(int supplierId) =>
	    Error.NotFound("Purchase.SupplierNotFound", $"Supplier {supplierId} does not exist.");
    
}
