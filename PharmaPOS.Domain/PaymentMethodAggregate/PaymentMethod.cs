using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.Common.Enums;

namespace PharmaPOS.Domain.PaymentMethodAggregate;

public class PaymentMethod : EntityWithDate
{
	// EF Core parameterless constructor
	private PaymentMethod() { }

	public PaymentMethod(PaymentMethodDetails details)
	{
		Update(details);
	}

	public string Name { get; private set; } = null!;
	public Status Status { get; private set; }

	public void Update(PaymentMethodDetails details)
	{
		if (string.IsNullOrWhiteSpace(details.Name))
			throw new ArgumentException("Payment Method Name is required.", nameof(details.Name));
		if (!Enum.IsDefined(details.Status))
			throw new ArgumentException("Invalid status value.", nameof(details.Status));

		Name = details.Name;
		Status = details.Status;
	}
}
