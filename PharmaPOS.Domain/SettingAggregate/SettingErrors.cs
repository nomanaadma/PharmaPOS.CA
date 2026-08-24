using ErrorOr;

namespace PharmaPOS.Domain.SettingAggregate;

public static class SettingErrors
{
	public static Error NameRequired =>
		Error.Validation("Setting.NameRequired", "Store Name is required.");

	public static Error LegalNameRequired =>
		Error.Validation("Setting.LegalNameRequired", "Store Legal Name is required.");

	public static Error MailRequired =>
		Error.Validation("Setting.MailRequired", "Email Address is required.");

	public static Error PhoneRequired =>
		Error.Validation("Setting.PhoneRequired", "Phone Number is required.");
}
