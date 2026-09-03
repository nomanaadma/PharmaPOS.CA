using PharmaPOS.Application.IProviders;

namespace PharmaPOS.Razor.Providers;

public class FilePathProvider(IWebHostEnvironment env) : IFilePathProvider
{
	public string GetUploadsFilePath(string fileName)
	{
		return string.IsNullOrEmpty(fileName) ? string.Empty : Path.Combine(env.WebRootPath, "uploads", fileName);
	}
}