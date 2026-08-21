using PharmaPOS.Domain.Common.Entities;

namespace PharmaPOS.Domain.SettingAggregate;

public class Setting : Entity
{
	// EF Core parameterless constructor
	private Setting() { }

	public Setting(SettingDetails details)
	{
		Update(details);
		Address = Address.Empty;
	}

	public string? Logo { get; private set; }
	public string? LogoIcon { get; private set; }
	public string? LgBackground { get; private set; }
	public string? Favicon { get; private set; }
	public string Name { get; private set; } = null!;
	public string LegalName { get; private set; } = null!;
	public string Mail { get; private set; } = null!;
	public string Phone { get; private set; } = null!;
	public Address Address { get; private set; } = null!;

	public void Update(SettingDetails details)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Name);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.LegalName);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Mail);
		ArgumentException.ThrowIfNullOrWhiteSpace(details.Phone);

		Name = details.Name;
		LegalName = details.LegalName;
		Mail = details.Mail;
		Phone = details.Phone;
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
