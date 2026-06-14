using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Domain.ValueObjects;
using PharmaPOS_MVC.Models;
using System.Text.Json;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
public class SettingController(ApplicationDbContext context) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var settings = await context.Settings.FirstOrDefaultAsync();
		if (settings == null)
		{
			settings = new Setting("PharmaPOS Store", "PharmaPOS Store LLC", "store@pharmapos.com", "1234567890");
			context.Settings.Add(settings);
			await context.SaveChangesAsync();
		}

		var model = new SystemInfoViewModel
		{
			Logo = settings.Logo,
			LogoIcon = settings.LogoIcon,
			LgBackground = settings.LgBackground,
			Favicon = settings.Favicon,
			Name = settings.Name,
			LegalName = settings.LegalName,
			Mail = settings.Mail,
			Phone = settings.Phone,
			AddressLine1 = settings.Address?.Line1,
			AddressLine2 = settings.Address?.Line2,
			City = settings.Address?.City,
			Country = settings.Address?.Country,
			Postal = settings.Address?.Postal
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
			return View(model);
		}

		var settings = await context.Settings.FirstOrDefaultAsync();
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
