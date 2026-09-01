using ErrorOr;

namespace PharmaPOS.Domain.PurchaseAggregate;

public static class PurchaseErrors
{
    public static Error NotFound(int id) =>
        Error.NotFound("Purchase.NotFound", $"Purchase {id} does not exist.");
    
    public static Error MissingItems =>
	    Error.Validation("Purchase.MissingItems", "Purchase must have at least one item.");
    
}
