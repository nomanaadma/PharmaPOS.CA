namespace PharmaPOS.Domain.Entities;

public class Media : EntityWithDate
{
	// EF Core parameterless constructor
	private Media() { }

	public Media(string filename, string ext, DateTime createdDate)
	{
		if (string.IsNullOrWhiteSpace(filename))
			throw new ArgumentException("Filename cannot be empty.", nameof(filename));
		if (string.IsNullOrWhiteSpace(ext))
			throw new ArgumentException("Extension cannot be empty.", nameof(ext));

		Filename = filename;
		Ext = ext;
		CreatedDate = createdDate;
	}

	public string Filename { get; private set; } = null!;
	public string Ext { get; private set; } = null!;
}
