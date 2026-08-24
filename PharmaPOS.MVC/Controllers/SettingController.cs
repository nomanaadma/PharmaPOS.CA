using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Models;
using PharmaPOS.Domain.SettingAggregate;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class SettingController(
	ApplicationDbContext context,
	SiteInfoService siteInfoService
) : BaseController
{
	[HttpGet]
	[Authorize(Policy = Permissions.Info.View)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var siteInfo = await siteInfoService.GetSettingsAsync();
		var rawSetting = await context.Settings.FirstOrDefaultAsync(ct);

		var model = SettingMapper.MapToSystemInfo(rawSetting, siteInfo);
		
		return View("Info", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Info.View)]
	public async Task<IActionResult> Index(SystemInfoViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
			NotifyError("Validation failed: " + errors);
			return View("Info", model);
		}

		var settings = await context.Settings.FirstOrDefaultAsync(ct);
		var details = new SettingDetails(model.Name, model.LegalName, model.Mail, model.Phone);
		
		if (settings == null)
		{
			var settingsResult = Setting.Create(details);
			if (settingsResult.IsError)
			{
				NotifyError(settingsResult.Errors[0].Description);
				return View("Info", model);
			}
			settings = settingsResult.Value;
			context.Settings.Add(settings);
		}
		else
		{
			var updateResult = settings.Update(details);
			if (updateResult.IsError)
			{
				NotifyError(updateResult.Errors[0].Description);
				return View("Info", model);
			}
		}

		settings.UpdateImages(model.Logo, model.LogoIcon, model.LgBackground, model.Favicon);
		settings.UpdateAddress(new Address(model.AddressLine1, model.AddressLine2, model.City, model.Country, model.Postal));

		await context.SaveChangesAsync(ct);

		NotifySuccess("System settings updated successfully.");

		return RedirectToAction(nameof(Index));
	}
}
