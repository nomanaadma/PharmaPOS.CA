using ErrorOr;

namespace PharmaPOS.Domain.PaymentMethodAggregate;

public static class PaymentMethodErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("PaymentMethod.NotFound", $"Payment method {id} does not exist.");

	public static readonly Error NameNotUnique =
		Error.Conflict("PaymentMethod.NameNotUnique", "Payment Method Name must be unique.");
}
