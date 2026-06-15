using PharmaPOS_MVC.Domain.Enums;

namespace PharmaPOS_MVC.Domain.Entities;

public class PaymentMethod
{
	// EF Core parameterless constructor
	private PaymentMethod() { }

	public PaymentMethod(string name, PaymentMethodStatus status)
	{
		Update(name, status);
		CreatedDate = DateTime.UtcNow;
	}

	public int Id { get; private set; }
	public string Name { get; private set; } = null!;
	public PaymentMethodStatus Status { get; private set; }
	public DateTime CreatedDate { get; private set; }

	public void Update(string name, PaymentMethodStatus status)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Payment Method Name is required.", nameof(name));
		if (!Enum.IsDefined(status))
			throw new ArgumentException("Invalid status value.", nameof(status));

		Name = name;
		Status = status;
	}
}
