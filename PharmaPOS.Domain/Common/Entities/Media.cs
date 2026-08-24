namespace PharmaPOS.Domain.Common.Entities;

public class Media : EntityWithDate
{
	// EF Core parameterless constructor
	private Media() { }

	public Media(string filename, string ext, DateTime createdDate)
	{
		Filename = filename;
		Ext = ext;
		CreatedDate = createdDate;
	}

	public string Filename { get; private set; } = null!;
	public string Ext { get; private set; } = null!;
}
