using PharmaPOS.Domain.Enums;

namespace PharmaPOS.Domain.Entities;

public class PaymentMethod : EntityWithDate
{
	// EF Core parameterless constructor
	private PaymentMethod() { }

	public PaymentMethod(string name, Status status)
	{
		Update(name, status);
	}

	public string Name { get; private set; } = null!;
	public Status Status { get; private set; }

	public void Update(string name, Status status)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Payment Method Name is required.", nameof(name));
		if (!Enum.IsDefined(status))
			throw new ArgumentException("Invalid status value.", nameof(status));

		Name = name;
		Status = status;
	}
}
