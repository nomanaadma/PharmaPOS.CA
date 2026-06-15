using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Domain.ValueObjects;
using PharmaPOS_MVC.Models;
using System.Text.Json;
using PharmaPOS_MVC.Services;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
public class SettingController(
	ApplicationDbContext context,
	SiteInfoService siteInfoService
) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var siteInfo = await siteInfoService.GetSettingsAsync();
		var rawSetting = await context.Settings.FirstOrDefaultAsync();

		var model = new SystemInfoViewModel
		{
			Logo = rawSetting?.Logo,
			LogoIcon = rawSetting?.LogoIcon,
			LgBackground = rawSetting?.LgBackground,
			Favicon = rawSetting?.Favicon,
			Name = siteInfo.Name,
			LegalName = siteInfo.LegalName,
			Mail = siteInfo.Mail,
			Phone = siteInfo.Phone,
			AddressLine1 = siteInfo.Address?.Line1,
			AddressLine2 = siteInfo.Address?.Line2,
			City = siteInfo.Address?.City,
			Country = siteInfo.Address?.Country,
			Postal = siteInfo.Address?.Postal
		};

		return View("Info", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Index(SystemInfoViewModel model)
	{
		if (!ModelState.IsValid)
		{
			var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Validation failed: " + errors });
			return View("Info", model);
		}

		var settings = await context.Settings.AsTracking().FirstOrDefaultAsync();
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

		await context.SaveChangesAsync();

		TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "System settings updated successfully." });

		return RedirectToAction(nameof(Index));
	}
}
