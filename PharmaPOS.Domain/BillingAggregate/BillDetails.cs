namespace PharmaPOS.Domain.BillingAggregate;

public record BillDetails(int CustomerId, string Name, string? Email, string? Mobile, int MethodId, DateTime BillDate, decimal SubTotal, decimal Tax, decimal Discount, decimal Amount, string? Note);
