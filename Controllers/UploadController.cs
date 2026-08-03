using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Extensions;
using IOFile = System.IO.File;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("media")]
public class UploadController(IWebHostEnvironment env, ApplicationDbContext dbContext) : Controller
{
	[HttpGet]
	[Route("get")]
	[Authorize(Policy = Permissions.Media.List)]
	public async Task<IActionResult> Get()
	{
		var result = await dbContext.Media.OrderBy(m => m.CreatedDate).ToListAsync();
		var html = "";
		
		foreach (var value in result)
		{
			var filePath = env.UploadsFilePath(value.Filename);
			var (width, height) = GetImageDimensions(filePath);

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

	[HttpPost]
	[Route("upload")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Media.Upload)]
	public async Task<IActionResult> Upload(IFormFile? file)
	{
		if (file == null || file.Length == 0)
		{
			return Json(new { error = true, message = "No file uploaded." });
		}

		var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLower();

		var uniqueId = Guid.NewGuid().ToString("N")[..8];
		var fileName = $"media-{uniqueId}.{ext}";
		var filePath = env.UploadsFilePath(fileName);

		await using (var stream = new FileStream(filePath, FileMode.Create))
		{
			await file.CopyToAsync(stream);
		}

		var media = new Media(fileName, ext, DateTime.UtcNow);
		dbContext.Media.Add(media);
		await dbContext.SaveChangesAsync();

		var (width, height) = GetImageDimensions(filePath);
		
		var canDelete = User.HasPermission(Permissions.Media.Delete);

		var deleteHtml = canDelete
			? """<a class="remove" data-toggle="tooltip" data-placement="top" title="Remove"><i class="las la-trash-alt"></i></a>"""
			: string.Empty;

		var mediaHtml = $"""
		                 <div class="block">
		                     {deleteHtml}
		                     <input type="radio" class="block-id" value="{media.Id}">
		                     <div class="picture">
		                         <img src="/uploads/{fileName}" title="{fileName}">
		                         <input type="radio" id="{fileName}" value="{fileName}">
		                         <label for="{fileName}">{fileName}</label>
		                     </div>
		                     <label class="font-12">Image Size : {width}*{height}</label>
		                 </div>
		                 """;

		return Json(new { error = false, name = fileName, id = media.Id, media = mediaHtml });
	}

	[HttpPost]
	[Route("delete")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Media.Delete)]
	public async Task<IActionResult> Delete(string name, int id)
	{
		if (string.IsNullOrEmpty(name))
		{
			return Json(new { error = true, message = "File name is wrong or does not exist." });
		}

		var media = await dbContext.Media.FirstOrDefaultAsync(m => m.Id == id && m.Filename == name);
		if (media == null)
		{
			return Json(new { error = true, message = "File is wrong or does not exist." });
		}

		var filePath = env.UploadsFilePath(name);
		if (IOFile.Exists(filePath))
		{
			try
			{
				IOFile.Delete(filePath);
			}
			catch (Exception ex)
			{
				return Json(new { error = true, message = "Error deleting " + name + ": " + ex.Message });
			}
		}

		dbContext.Media.Remove(media);
		await dbContext.SaveChangesAsync();

		return Json(new { error = false, message = "File deleted successfully." });
	}
	

	private static (int width, int height) GetImageDimensions(string path)
	{
		try
		{
			if (!IOFile.Exists(path)) return (0, 0);

			using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new BinaryReader(fileStream);
			
			var header = reader.ReadBytes(8);
			if (header.Length < 8) return (0, 0);

			// Check PNG: 89 50 4E 47 0D 0A 1A 0A
			if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
			{
				fileStream.Position = 16;
				var widthBytes = reader.ReadBytes(4);
				var heightBytes = reader.ReadBytes(4);
				Array.Reverse(widthBytes); // big-endian
				Array.Reverse(heightBytes);
				var width = BitConverter.ToInt32(widthBytes, 0);
				var height = BitConverter.ToInt32(heightBytes, 0);
				return (width, height);
			}

			// Check GIF: GIF87a or GIF89a
			if (header[0] == 'G' && header[1] == 'I' && header[2] == 'F')
			{
				fileStream.Position = 6;
				int width = reader.ReadUInt16();
				int height = reader.ReadUInt16();
				return (width, height);
			}

			// Check JPEG: FF D8
			if (header[0] == 0xFF && header[1] == 0xD8)
			{
				fileStream.Position = 2;
				while (fileStream.Position < fileStream.Length)
				{
					var marker = reader.ReadByte();
					while (marker == 0xFF && fileStream.Position < fileStream.Length)
					{
						marker = reader.ReadByte();
					}

					if (marker is 0xD9 or 0xDA) // EOI or SOS
					{
						break;
					}

					var lenBytes = reader.ReadBytes(2);
					if (lenBytes.Length < 2) break;
					Array.Reverse(lenBytes);
					var length = BitConverter.ToUInt16(lenBytes, 0);

					if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or >= 0xC5 and <= 0xC7 or >= 0xC9 and <= 0xCB or >= 0xCD and <= 0xCF)
					{
						fileStream.Position += 1; // skip precision
						var hBytes = reader.ReadBytes(2);
						var wBytes = reader.ReadBytes(2);
						Array.Reverse(hBytes);
						Array.Reverse(wBytes);
						int height = BitConverter.ToUInt16(hBytes, 0);
						int width = BitConverter.ToUInt16(wBytes, 0);
						return (width, height);
					}

					fileStream.Position += (length - 2);
				}
			}
		}
		catch
		{
			// ignored
		}
		return (0, 0);
	}
}
