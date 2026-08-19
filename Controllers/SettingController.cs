using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS.Domain.Entities;
using PharmaPOS.Domain.ValueObjects;
using PharmaPOS_MVC.Models;
using System.Text.Json;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Mappers;
using PharmaPOS_MVC.Services;

namespace PharmaPOS_MVC.Controllers;

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
			settings = new Setting(model.Name, model.LegalName, model.Mail, model.Phone);
			context.Settings.Add(settings);
		}
		else
		{
			settings.Update(model.Name, model.LegalName, model.Mail, model.Phone);
		}

		settings.UpdateImages(model.Logo, model.LogoIcon, model.LgBackground, model.Favicon);
		settings.UpdateAddress(new Address(model.AddressLine1, model.AddressLine2, model.City, model.Country, model.Postal));

		await context.SaveChangesAsync(ct);

		NotifySuccess("System settings updated successfully.");

		return RedirectToAction(nameof(Index));
	}
}
