using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Models;
using System.Text.Json;
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
		if (settings == null)
		{
			var details = new SettingDetails(model.Name, model.LegalName, model.Mail, model.Phone);
			settings = new Setting(details);
			context.Settings.Add(settings);
		}
		else
		{
			var details = new SettingDetails(model.Name, model.LegalName, model.Mail, model.Phone);
			settings.Update(details);
		}

		settings.UpdateImages(model.Logo, model.LogoIcon, model.LgBackground, model.Favicon);
		settings.UpdateAddress(new Address(model.AddressLine1, model.AddressLine2, model.City, model.Country, model.Postal));

		await context.SaveChangesAsync(ct);

		NotifySuccess("System settings updated successfully.");

		return RedirectToAction(nameof(Index));
	}
}
