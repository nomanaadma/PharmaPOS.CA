using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.IProviders;
using PharmaPOS.Application.Services;
using PharmaPOS.MVC.Extensions;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
[Route("media")]
public class UploadController(IFilePathProvider pathProvider, SettingService settingService) : BaseController
{
	[HttpGet("get")]
	[Authorize(Policy = Permissions.Media.List)]
	public async Task<IActionResult> Get(CancellationToken ct = default)
	{
		var result = await settingService.GetMediaListAsync(ct);
		var html = "";
		
		foreach (var value in result)
		{
			var filePath = pathProvider.GetUploadsFilePath(value.Filename);
			var (width, height) = settingService.GetImageDimensions(filePath);

			html += $"""
			 <div class="block">
			 <a class="remove" data-toggle="tooltip" data-placement="top" title="Remove"><i class="las la-trash-alt"></i></a>
			 <input type="hidden" class="block-id" value="{value.Id}">
			 <div class="picture">
			 <img src="/uploads/{value.Filename}" title="{value.Filename}">
			 <input type="radio" id="{value.Filename}" value="{value.Filename}">
			 <label for="{value.Filename}">{value.Filename}</label>
			 </div>
			 <label class="font-12">Image Size : {width}*{height}</label>
			 </div>
			 """;
		}

		return Content(html, "text/html");
	}

	[HttpPost("upload")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Media.Upload)]
	public async Task<IActionResult> Upload(IFormFile? file, CancellationToken ct = default)
	{
		if (file == null || file.Length == 0)
		{
			return Json(new { error = true, message = "No file uploaded." });
		}

		var uploadResult = await settingService.UploadMediaFileAsync(file, ct);
		if (uploadResult.IsError)
			return Json(new { error = true, message = uploadResult.Errors[0].Description });

		var (media, width, height) = uploadResult.Value;
		
		var canDelete = User.HasPermission(Permissions.Media.Delete);

		var deleteHtml = canDelete
			? """<a class="remove" data-toggle="tooltip" data-placement="top" title="Remove"><i class="las la-trash-alt"></i></a>"""
			: string.Empty;

		var mediaHtml = $"""
		                 <div class="block">
		                     {deleteHtml}
		                     <input type="radio" class="block-id" value="{media.Id}">
		                     <div class="picture">
		                         <img src="/uploads/{media.Filename}" title="{media.Filename}">
		                         <input type="radio" id="{media.Filename}" value="{media.Filename}">
		                         <label for="{media.Filename}">{media.Filename}</label>
		                     </div>
		                     <label class="font-12">Image Size : {width}*{height}</label>
		                 </div>
		                 """;

		return Json(new { error = false, name = media.Filename, id = media.Id, media = mediaHtml });
	}

	[HttpPost("delete")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Media.Delete)]
	public async Task<IActionResult> Delete(string name, int id, CancellationToken ct = default)
	{
		if (string.IsNullOrEmpty(name))
		{
			return Json(new { error = true, message = "File name is wrong or does not exist." });
		}

		var result = await settingService.DeleteMediaFileAsync(id, name, ct);
		return Json(result.IsError ? 
			new { error = true, message = result.Errors[0].Description } : 
			new { error = false, message = "File deleted successfully." });
	}
}
