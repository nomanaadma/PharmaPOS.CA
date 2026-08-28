using ErrorOr;

namespace PharmaPOS.Domain.MediaAggregate;

public static class MediaErrors
{
	public static Error FilenameRequired =>
		Error.Validation("Media.FilenameRequired", "Filename cannot be empty.");

	public static Error ExtensionRequired =>
		Error.Validation("Media.ExtensionRequired", "Extension cannot be empty.");
}
