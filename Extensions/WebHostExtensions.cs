namespace PharmaPOS_MVC.Extensions;

public static class WebHostExtensions
{
	extension(IWebHostEnvironment webHost)
	{
		public string UploadsDir()
		{
			return Path.Combine(webHost.WebRootPath, "uploads");
		}

		public string UploadsFilePath(string fileName)
		{
			return Path.Combine(webHost.UploadsDir(), fileName);
		}
	}
}