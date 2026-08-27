namespace PharmaPOS.Application.IProviders;

public interface IFilePathProvider
{
	string GetUploadsFilePath(string fileName);
}

