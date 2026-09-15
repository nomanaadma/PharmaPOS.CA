using ErrorOr;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Application.IProviders;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.MediaAggregate;
using PharmaPOS.Domain.SettingAggregate;

namespace PharmaPOS.Application.Services;

public class SettingService(
	ISettingRepository settingRepo,
	IMediaRepository mediaRepo,
	IUnitOfWork unitOfWork,
	IFilePathProvider pathProvider) : IApplicationService
{
	public async Task<Setting?> GetRawSettingsAsync(CancellationToken ct = default)
	{
		return await settingRepo.GetRawSettingsAsync(ct);
	}

	public async Task<Setting> GetSettingsAsync(CancellationToken ct = default)
	{
		var settings = await settingRepo.GetSettingsAsync(ct);
	
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
		if (!string.IsNullOrEmpty(settings.Logo) && File.Exists(pathProvider.GetUploadsFilePath(settings.Logo)))
		{
			logo = $"/uploads/{settings.Logo}";
		}
		
		var logoIcon = "/images/icon.png";
		if (!string.IsNullOrEmpty(settings.LogoIcon) && File.Exists(pathProvider.GetUploadsFilePath(settings.LogoIcon)))
		{
			logoIcon = $"/uploads/{settings.LogoIcon}";
		}
		
		var favicon = "/images/favicon.png";
		if (!string.IsNullOrEmpty(settings.Favicon) && File.Exists(pathProvider.GetUploadsFilePath(settings.Favicon)))
		{
			favicon = $"/uploads/{settings.Favicon}";
		}
		
		var lgBackground = "/images/login-bg.jpg";
		if (!string.IsNullOrEmpty(settings.LgBackground) && File.Exists(pathProvider.GetUploadsFilePath(settings.LgBackground)))
		{
			lgBackground = $"/uploads/{settings.LgBackground}";
		}
		
		settings.UpdateImages(logo, logoIcon, lgBackground, favicon);
		
		return settings;
	}

	public async Task<ErrorOr<Updated>> UpdateSettingsAsync(SettingRequest request, CancellationToken ct = default)
	{
		var settings = await settingRepo.GetRawSettingsAsync(ct);
		var details = new SettingDetails(request.Name, request.LegalName, request.Mail, request.Phone);
		
		if (settings == null)
		{
			var settingsResult = Setting.Create(details);
			if (settingsResult.IsError)
			{
				return settingsResult.Errors;
			}
			settings = settingsResult.Value;
			await settingRepo.AddAsync(settings, ct);
		}
		else
		{
			var updateResult = settings.Update(details);
			if (updateResult.IsError)
			{
				return updateResult.Errors;
			}
		}

		settings.UpdateImages(request.Logo, request.LogoIcon, request.LgBackground, request.Favicon);
		settings.UpdateAddress(new Address(request.AddressLine1, request.AddressLine2, request.City, request.Country, request.Postal));

		await unitOfWork.SaveChangesAsync(ct);
		return Result.Updated;
	}

	public async Task<List<Media>> GetMediaListAsync(CancellationToken ct = default)
	{
		return await mediaRepo.GetMediaListAsync(ct);
	}

	public async Task<ErrorOr<MediaUploadResponse>> UploadMediaFileAsync(Stream fileStream, string originalFileName, long length, CancellationToken ct = default)
	{
		if (length == 0)
			return Error.Validation(description: "No file uploaded.");

		var ext = Path.GetExtension(originalFileName).TrimStart('.').ToLower();
		var uniqueId = Guid.NewGuid().ToString("N")[..8];
		var fileName = $"media-{uniqueId}.{ext}";
		var filePath = pathProvider.GetUploadsFilePath(fileName);

		await using (var destinationStream = new FileStream(filePath, FileMode.Create))
		{
			await fileStream.CopyToAsync(destinationStream, ct);
		}

		var mediaResult = Media.Create(fileName, ext, DateTime.UtcNow);
		if (mediaResult.IsError)
			return mediaResult.Errors;

		var media = mediaResult.Value;
		await mediaRepo.AddAsync(media, ct);
		await unitOfWork.SaveChangesAsync(ct);

		var (width, height) = GetImageDimensions(filePath);

		return new MediaUploadResponse(media, width, height);
	}

	public async Task<ErrorOr<Deleted>> DeleteMediaFileAsync(int id, string name, CancellationToken ct = default)
	{
		var media = await mediaRepo.GetByIdAndFilenameAsync(id, name, ct);
		if (media == null)
			return Error.NotFound(description: "File is wrong or does not exist.");

		var filePath = pathProvider.GetUploadsFilePath(name);
		if (File.Exists(filePath))
		{
			try
			{
				File.Delete(filePath);
			}
			catch (Exception ex)
			{
				return Error.Failure(description: "Error deleting " + name + ": " + ex.Message);
			}
		}

		mediaRepo.Remove(media);
		await unitOfWork.SaveChangesAsync(ct);

		return Result.Deleted;
	}

	public (int width, int height) GetImageDimensions(string path)
	{
		try
		{
			if (!File.Exists(path)) return (0, 0);

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