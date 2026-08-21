namespace PharmaPOS.Domain.PurchaseAggregate;

public record PurchaseDetails(int SupplierId, DateTime Date, decimal SubTotal, decimal Tax, decimal Discount, decimal Amount, string? Note);
