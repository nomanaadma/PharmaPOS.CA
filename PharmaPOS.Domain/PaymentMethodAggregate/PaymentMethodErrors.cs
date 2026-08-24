using ErrorOr;

namespace PharmaPOS.Domain.PaymentMethodAggregate;

public static class PaymentMethodErrors
{
	public static Error NotFound(int id) =>
		Error.NotFound("PaymentMethod.NotFound", $"Payment method {id} does not exist.");

	public static Error NameRequired =>
		Error.Validation("PaymentMethod.NameRequired", "Payment Method Name is required.");

	public static Error InvalidStatus =>
		Error.Validation("PaymentMethod.InvalidStatus", "Invalid status value.");

	public static Error NameNotUnique =>
		Error.Conflict("PaymentMethod.NameNotUnique", "Payment Method Name must be unique.");
}
