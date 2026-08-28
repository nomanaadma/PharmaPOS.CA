using ErrorOr;
using PharmaPOS.Domain.Common;

namespace PharmaPOS.Domain.MediaAggregate;

public class Media : Entity
{
	// EF Core parameterless constructor
	private Media() { }

	private Media(string filename, string ext, DateTime createdDate)
	{
		Filename = filename;
		Ext = ext;
		CreatedDate = createdDate;
	}

	public static ErrorOr<Media> Create(string filename, string ext, DateTime createdDate)
	{
		if (string.IsNullOrWhiteSpace(filename))
			return MediaErrors.FilenameRequired;
		if (string.IsNullOrWhiteSpace(ext))
			return MediaErrors.ExtensionRequired;

		return new Media(filename, ext, createdDate);
	}

	public string Filename { get; private set; } = null!;
	public string Ext { get; private set; } = null!;
	
	public DateTime CreatedDate { get; private init; }
}
