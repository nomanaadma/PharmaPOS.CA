using PharmaPOS.Application.Data;
using PharmaPOS.Domain.Common.Entities;
using PharmaPOS.Domain.SettingAggregate;

namespace PharmaPOS.MVC.Services;

using Extensions;
using Microsoft.EntityFrameworkCore;
using Data;
using PharmaPOS.MVC.Models;
using ErrorOr;
using IOFile = System.IO.File;

public class SettingService(ApplicationDbContext dbContext, IWebHostEnvironment env)
{

	public async Task<Setting?> GetRawSettingsAsync(CancellationToken ct = default)
	{
		return await dbContext.Settings.FirstOrDefaultAsync(ct);
	}

	public async Task<Setting> GetSettingsAsync(CancellationToken ct = default)
	{
		
		var settings = await dbContext.Settings.AsNoTracking().OrderBy(s => s.Id).FirstOrDefaultAsync(cancellationToken: ct);
	
		// 1. If records are not found, create a new Setting and Address object with default values
		if (settings == null)
		{
			var details = new SettingDetails("PharmaPOS Store", "PharmaPOS Store LLC", "store@pharmapos.com", "1234567890");
			var settingsResult = Setting.Create(details);
			if (settingsResult.IsError) throw new Exception("Failed to initialize default settings");
			settings = settingsResult.Value;
			settings.UpdateAddress(Address.Empty);
		}
		
		var logo = "/images/logo.png";
		
		if (!string.IsNullOrEmpty(settings.Logo) && File.Exists( env.UploadsFilePath(settings.Logo) ))
		{
			logo = $"/uploads/{settings.Logo}";
		}
		
		
		var logoIcon = "/images/icon.png";

		if (!string.IsNullOrEmpty(settings.LogoIcon) && File.Exists( env.UploadsFilePath(settings.LogoIcon) ))
		{
			logoIcon = $"/uploads/{settings.LogoIcon}";
		}
		
		var favicon = "/images/favicon.png";

		if (!string.IsNullOrEmpty(settings.Favicon) && File.Exists( env.UploadsFilePath(settings.Favicon) ))
		{
			favicon = $"/uploads/{settings.Favicon}";
		}
		
		var lgBackground = "/images/login-bg.jpg";

		if (!string.IsNullOrEmpty(settings.LgBackground) && File.Exists( env.UploadsFilePath(settings.LgBackground) ))
		{
			lgBackground = $"/uploads/{settings.LgBackground}";
		}
		
		settings.UpdateImages(logo, logoIcon, lgBackground, favicon);
		
		return settings;
		
		
	}

	public async Task<ErrorOr<Success>> UpdateSettingsAsync(SystemInfoViewModel model, CancellationToken ct = default)
	{
		var settings = await dbContext.Settings.FirstOrDefaultAsync(ct);
		var details = new SettingDetails(model.Name, model.LegalName, model.Mail, model.Phone);
		
		if (settings == null)
		{
			var settingsResult = Setting.Create(details);
			if (settingsResult.IsError)
			{
				return settingsResult.Errors;
			}
			settings = settingsResult.Value;
			dbContext.Settings.Add(settings);
		}
		else
		{
			var updateResult = settings.Update(details);
			if (updateResult.IsError)
			{
				return updateResult.Errors;
			}
		}

		settings.UpdateImages(model.Logo, model.LogoIcon, model.LgBackground, model.Favicon);
		settings.UpdateAddress(new Address(model.AddressLine1, model.AddressLine2, model.City, model.Country, model.Postal));

		await dbContext.SaveChangesAsync(ct);
		return Result.Success;
	}

	public async Task<List<Media>> GetMediaListAsync(CancellationToken ct = default)
	{
		return await dbContext.Media.OrderBy(m => m.CreatedDate).ToListAsync(ct);
	}

	public async Task<ErrorOr<(Media Media, int Width, int Height)>> UploadMediaFileAsync(IFormFile file, CancellationToken ct = default)
	{
		if (file.Length == 0)
			return Error.Validation(description: "No file uploaded.");

		var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
		var uniqueId = Guid.NewGuid().ToString("N")[..8];
		var fileName = $"media-{uniqueId}.{ext}";
		var filePath = env.UploadsFilePath(fileName);

		await using (var stream = new FileStream(filePath, FileMode.Create))
		{
			await file.CopyToAsync(stream, ct);
		}

		var mediaResult = Media.Create(fileName, ext, DateTime.UtcNow);
		if (mediaResult.IsError)
			return mediaResult.Errors;

		var media = mediaResult.Value;
		dbContext.Media.Add(media);
		await dbContext.SaveChangesAsync(ct);

		var (width, height) = GetImageDimensions(filePath);

		return (media, width, height);
	}

	public async Task<ErrorOr<Success>> DeleteMediaFileAsync(int id, string name, CancellationToken ct = default)
	{
		var media = await dbContext.Media.FirstOrDefaultAsync(m => m.Id == id && m.Filename == name, ct);
		if (media == null)
			return Error.NotFound(description: "File is wrong or does not exist.");

		var filePath = env.UploadsFilePath(name);
		if (IOFile.Exists(filePath))
		{
			try
			{
				IOFile.Delete(filePath);
			}
			catch (Exception ex)
			{
				return Error.Failure(description: "Error deleting " + name + ": " + ex.Message);
			}
		}

		dbContext.Media.Remove(media);
		await dbContext.SaveChangesAsync(ct);

		return Result.Success;
	}

	public (int width, int height) GetImageDimensions(string path)
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