using PharmaPOS_MVC.Domain.ValueObjects;

namespace PharmaPOS_MVC.Domain.Entities;

public class Setting
{
	// EF Core parameterless constructor
	private Setting() { }

	public Setting(string name, string legalName, string mail, string phone)
	{
		Update(name, legalName, mail, phone);
		Address = Address.Empty;
	}

	public int Id { get; private set; }
	public string? Logo { get; private set; }
	public string? LogoIcon { get; private set; }
	public string? LgBackground { get; private set; }
	public string? Favicon { get; private set; }
	public string Name { get; private set; } = null!;
	public string LegalName { get; private set; } = null!;
	public string Mail { get; private set; } = null!;
	public string Phone { get; private set; } = null!;
	public Address Address { get; private set; } = null!;

	public void Update(string name, string legalName, string mail, string phone)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Store Name is required.", nameof(name));
		if (string.IsNullOrWhiteSpace(legalName))
			throw new ArgumentException("Store Legal Name is required.", nameof(legalName));
		if (string.IsNullOrWhiteSpace(mail))
			throw new ArgumentException("Email Address is required.", nameof(mail));
		if (string.IsNullOrWhiteSpace(phone))
			throw new ArgumentException("Phone Number is required.", nameof(phone));

		Name = name;
		LegalName = legalName;
		Mail = mail;
		Phone = phone;
	}

	public void UpdateImages(string? logo, string? logoIcon, string? lgBackground, string? favicon)
	{
		Logo = logo;
		LogoIcon = logoIcon;
		LgBackground = lgBackground;
		Favicon = favicon;
	}

	public void UpdateAddress(Address address)
	{
		Address = address ?? throw new ArgumentNullException(nameof(address));
	}
}
