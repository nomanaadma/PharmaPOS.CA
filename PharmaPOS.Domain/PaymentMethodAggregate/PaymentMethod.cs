using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.Common.Guards;

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
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Name);
		Guard.ThrowIfUndefined(details.Status);
		
		Name = details.Name;
		Status = details.Status;
	}
}
