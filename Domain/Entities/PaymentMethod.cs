using System;

namespace PharmaPOS_MVC.Domain.Entities;

public class PaymentMethod
{
	// EF Core parameterless constructor
	private PaymentMethod() { }

	public PaymentMethod(string name, int status)
	{
		Update(name, status);
		CreatedDate = DateTime.UtcNow;
	}

	public int Id { get; private set; }
	public string Name { get; private set; } = null!;
	public int Status { get; private set; }
	public DateTime CreatedDate { get; private set; }

	public void Update(string name, int status)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Payment Method Name is required.", nameof(name));
		if (status != 0 && status != 1)
			throw new ArgumentException("Invalid status value.", nameof(status));

		Name = name;
		Status = status;
	}
}
