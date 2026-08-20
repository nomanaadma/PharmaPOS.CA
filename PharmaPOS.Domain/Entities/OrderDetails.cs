namespace PharmaPOS.Domain.Entities;

public abstract class OrderDetails : Entity
{
	public DateTime Date { get; protected set; }
	public decimal Subtotal { get; protected set; }
	public decimal Tax { get; protected set; }
	public decimal DiscountValue { get; protected set; }
	public decimal Amount { get; protected set; }
	
	// ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
	public string? Note { get; protected set; }
	public DateTime CreatedDate { get; protected init; }
}