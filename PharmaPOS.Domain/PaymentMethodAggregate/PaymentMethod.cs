using ErrorOr;
using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.Common.Enums;

namespace PharmaPOS.Domain.PaymentMethodAggregate;

public class PaymentMethod : EntityWithDate, IAggregateRoot
{
	// EF Core parameterless constructor
	private PaymentMethod() { }

	private PaymentMethod(PaymentMethodDetails details)
	{
		SetDetails(details);
	}

	public string Name { get; private set; } = null!;
	public Status Status { get; private set; }

	public static ErrorOr<PaymentMethod> Create(PaymentMethodDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		return new PaymentMethod(details);
	}

	public ErrorOr<Updated> Update(PaymentMethodDetails details)
	{
		var errors = ValidateDetails(details);
		if (errors.Count > 0) return errors;

		SetDetails(details);
		return Result.Updated;
	}

	private void SetDetails(PaymentMethodDetails details)
	{
		Name = details.Name;
		Status = details.Status;
	}

	private static List<Error> ValidateDetails(PaymentMethodDetails details)
	{
		var errors = new List<Error>();

		if (string.IsNullOrWhiteSpace(details.Name))
			errors.Add(PaymentMethodErrors.NameRequired);
		if (!Enum.IsDefined(details.Status))
			errors.Add(PaymentMethodErrors.InvalidStatus);

		return errors;
	}
}
