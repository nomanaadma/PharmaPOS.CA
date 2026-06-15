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

	[HttpGet]
	public async Task<IActionResult> Suppliers()
	{
		var suppliers = await context.Suppliers.OrderBy(s => s.Id).ToListAsync();
		return View(suppliers);
	}

	[HttpGet]
	public IActionResult SupplierAdd()
	{
		return View("SupplierForm", new SupplierViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> SupplierAdd(SupplierViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View("SupplierForm", model);
		}

		if (await context.Suppliers.AnyAsync(s => s.Email == model.Email))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("SupplierForm", model);
		}

		try
		{
			var supplier = new Supplier(model.Name, model.Email, model.Phone, model.Address);
			context.Suppliers.Add(supplier);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Supplier created successfully." });
			return RedirectToAction(nameof(Suppliers));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error saving supplier: " + ex.Message });
			return View("SupplierForm", model);
		}
	}

	[HttpGet]
	public async Task<IActionResult> SupplierEdit(int id)
	{
		var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
		if (supplier == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Supplier not found." });
			return RedirectToAction(nameof(Suppliers));
		}

		var model = new SupplierViewModel
		{
			Id = supplier.Id,
			Name = supplier.Name,
			Email = supplier.Email,
			Phone = supplier.Phone,
			Address = supplier.Address
		};

		return View("SupplierForm", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> SupplierEdit(SupplierViewModel model)
	{
		if (!model.Id.HasValue)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Supplier ID is required for editing." });
			return RedirectToAction(nameof(Suppliers));
		}

		if (!ModelState.IsValid)
		{
			return View("SupplierForm", model);
		}

		if (await context.Suppliers.AnyAsync(s => s.Email == model.Email && s.Id != model.Id.Value))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("SupplierForm", model);
		}

		try
		{
			var supplier = await context.Suppliers.AsTracking().FirstOrDefaultAsync(s => s.Id == model.Id.Value);
			if (supplier == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Supplier not found." });
				return RedirectToAction(nameof(Suppliers));
			}

			supplier.Update(model.Name, model.Email, model.Phone, model.Address);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Supplier updated successfully." });
			return RedirectToAction(nameof(Suppliers));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error updating supplier: " + ex.Message });
			return View("SupplierForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> SupplierDelete(int id)
	{
		try
		{
			var supplier = await context.Suppliers.AsTracking().FirstOrDefaultAsync(s => s.Id == id);
			if (supplier == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Supplier not found." });
				return RedirectToAction(nameof(Suppliers));
			}

			context.Suppliers.Remove(supplier);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Supplier deleted successfully." });
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting supplier: " + ex.Message });
		}

		return RedirectToAction(nameof(Suppliers));
	}
}
