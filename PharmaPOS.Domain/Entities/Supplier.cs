using System;

namespace PharmaPOS.Domain.Entities;

public class Supplier : EntityWithDate
{
	// EF Core parameterless constructor
	private Supplier() { }

	public Supplier(string name, string email, string phone, string? address)
	{
		Update(name, email, phone, address);
	}

	public string Name { get; private set; } = null!;
	public string Email { get; private set; } = null!;
	public string Phone { get; private set; } = null!;
	public string? Address { get; private set; }

	public void Update(string name, string email, string phone, string? address)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Supplier Name is required.", nameof(name));
		if (string.IsNullOrWhiteSpace(email))
			throw new ArgumentException("Email Address is required.", nameof(email));
		if (string.IsNullOrWhiteSpace(phone))
			throw new ArgumentException("Phone Number is required.", nameof(phone));

		Name = name;
		Email = email;
		Phone = phone;
		Address = address;
	}
}
