using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
public class MediaController : Controller
{
	private readonly IWebHostEnvironment _env;

	public MediaController(IWebHostEnvironment env)
	{
		_env = env;
	}

	[HttpGet]
	[Route("get/media")]
	public IActionResult GetMedia()
	{
		var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
		if (!Directory.Exists(uploadsFolder))
		{
			Directory.CreateDirectory(uploadsFolder);
		}

		var files = Directory.GetFiles(uploadsFolder)
			.Select(Path.GetFileName)
			.Where(f => f != null)
			.ToList();

		var html = "<div class=\"row media-modal-open\">";
		foreach (var file in files)
		{
			html += $@"
<div class=""block col-md-2 col-sm-3 pb-3"">
	<div class=""picture card p-1"">
		<img src=""/public/uploads/{file}"" class=""img-thumbnail"" style=""cursor: pointer; max-height: 100px; object-fit: cover;"">
		<input type=""hidden"" value=""{file}"">
	</div>
	<input type=""hidden"" class=""block-id"" value=""{file}"">
	<a class=""remove btn btn-link text-danger p-0 d-block text-center mt-1""><i class=""las la-trash-alt""></i> Delete</a>
</div>";
		}
		html += "</div>";

		return Content(html, "text/html");
	}

	[HttpPost]
	[Route("media/upload")]
	public async Task<IActionResult> UploadMedia(IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			return Json(new { error = true, message = "No file uploaded." });
		}

		var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
		if (!Directory.Exists(uploadsFolder))
		{
			Directory.CreateDirectory(uploadsFolder);
		}

		var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
		var filePath = Path.Combine(uploadsFolder, fileName);

		using (var stream = new FileStream(filePath, FileMode.Create))
		{
			await file.CopyToAsync(stream);
		}

		var mediaHtml = $@"
<div class=""block col-md-2 col-sm-3 pb-3"">
	<div class=""picture card p-1"">
		<img src=""/public/uploads/{fileName}"" class=""img-thumbnail"" style=""cursor: pointer; max-height: 100px; object-fit: cover;"">
		<input type=""hidden"" value=""{fileName}"">
	</div>
	<input type=""hidden"" class=""block-id"" value=""{fileName}"">
	<a class=""remove btn btn-link text-danger p-0 d-block text-center mt-1""><i class=""las la-trash-alt""></i> Delete</a>
</div>";

		return Json(new { error = false, media = mediaHtml });
	}

	[HttpPost]
	[Route("media/delete")]
	public IActionResult DeleteMedia(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return Json(new { error = true, message = "File name not provided." });
		}

		var filePath = Path.Combine(_env.WebRootPath, "uploads", name);
		if (System.IO.File.Exists(filePath))
		{
			try
			{
				System.IO.File.Delete(filePath);
				return Json(new { error = false });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, message = ex.Message });
			}
		}

		return Json(new { error = true, message = "File not found." });
	}
}
