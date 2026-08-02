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

	[HttpGet]
	public async Task<IActionResult> Suppliers()
	{
		var suppliers = await context.Suppliers
			.OrderBy(s => s.Id)
			.Select(c => new SupplierViewModel()
			{
				Id = c.Id,
				Name = c.Name, 
				Email = c.Name, 
				Phone = c.Name, 
				Address = c.Address
			})
			.ToListAsync();
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
			var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == model.Id.Value);
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
			var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
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

	[HttpGet]
	public async Task<IActionResult> Categories()
	{
		var categories = await context.MedicineCategories
			.OrderBy(c => c.Id)
			.Select(c => new MedicineCategoryViewModel
			{
				Id = c.Id,
				Name = c.Name, 
			})
			.ToListAsync();
		
		return View(categories);
	}

	[HttpGet]
	public IActionResult CategoryAdd()
	{
		return View("CategoryForm", new MedicineCategoryViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CategoryAdd(MedicineCategoryViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View("CategoryForm", model);
		}

		if (await context.MedicineCategories.AnyAsync(c => c.Name == model.Name))
		{
			ModelState.AddModelError(nameof(model.Name), "Category Name must be unique.");
			return View("CategoryForm", model);
		}

		try
		{
			var category = new MedicineCategory(model.Name);
			context.MedicineCategories.Add(category);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Category created successfully." });
			return RedirectToAction(nameof(Categories));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error saving category: " + ex.Message });
			return View("CategoryForm", model);
		}
	}

	[HttpGet]
	public async Task<IActionResult> CategoryEdit(int id)
	{
		var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id);
		if (category == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Category not found." });
			return RedirectToAction(nameof(Categories));
		}

		var model = new MedicineCategoryViewModel
		{
			Id = category.Id,
			Name = category.Name
		};

		return View("CategoryForm", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CategoryEdit(MedicineCategoryViewModel model)
	{
		if (!model.Id.HasValue)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Category ID is required for editing." });
			return RedirectToAction(nameof(Categories));
		}

		if (!ModelState.IsValid)
		{
			return View("CategoryForm", model);
		}

		if (await context.MedicineCategories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id.Value))
		{
			ModelState.AddModelError(nameof(model.Name), "Category Name must be unique.");
			return View("CategoryForm", model);
		}

		try
		{
			var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == model.Id.Value);
			if (category == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Category not found." });
				return RedirectToAction(nameof(Categories));
			}

			category.Update(model.Name);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Category updated successfully." });
			return RedirectToAction(nameof(Categories));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error updating category: " + ex.Message });
			return View("CategoryForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CategoryDelete(int id)
	{
		try
		{
			var category = await context.MedicineCategories.FirstOrDefaultAsync(c => c.Id == id);
			if (category == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Category not found." });
				return RedirectToAction(nameof(Categories));
			}

			context.MedicineCategories.Remove(category);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Category deleted successfully." });
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting category: " + ex.Message });
		}

		return RedirectToAction(nameof(Categories));
	}

	[HttpGet]
	public async Task<IActionResult> PaymentMethods()
	{
		var methods = await context.PaymentMethods
			.OrderBy(pm => pm.Id)
			.Select(pm => new PaymentMethodViewModel
			{
				Id = pm.Id,
				Name = pm.Name,
				Status = pm.Status
			})
			.ToListAsync();
		
		return View(methods);
	}

	[HttpGet]
	public IActionResult PaymentMethodAdd()
	{
		return View("PaymentMethodForm", new PaymentMethodViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> PaymentMethodAdd(PaymentMethodViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View("PaymentMethodForm", model);
		}

		if (await context.PaymentMethods.AnyAsync(pm => pm.Name == model.Name))
		{
			ModelState.AddModelError(nameof(model.Name), "Payment Method Name must be unique.");
			return View("PaymentMethodForm", model);
		}

		try
		{
			var method = new PaymentMethod(model.Name, model.Status);
			context.PaymentMethods.Add(method);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Payment Method created successfully." });
			return RedirectToAction(nameof(PaymentMethods));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error saving payment method: " + ex.Message });
			return View("PaymentMethodForm", model);
		}
	}

	[HttpGet]
	public async Task<IActionResult> PaymentMethodEdit(int id)
	{
		var method = await context.PaymentMethods.FirstOrDefaultAsync(pm => pm.Id == id);
		if (method == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Payment Method not found." });
			return RedirectToAction(nameof(PaymentMethods));
		}

		var model = new PaymentMethodViewModel
		{
			Id = method.Id,
			Name = method.Name,
			Status = method.Status
		};

		return View("PaymentMethodForm", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> PaymentMethodEdit(PaymentMethodViewModel model)
	{
		if (!model.Id.HasValue)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Payment Method ID is required for editing." });
			return RedirectToAction(nameof(PaymentMethods));
		}

		if (!ModelState.IsValid)
		{
			return View("PaymentMethodForm", model);
		}

		if (await context.PaymentMethods.AnyAsync(pm => pm.Name == model.Name && pm.Id != model.Id.Value))
		{
			ModelState.AddModelError(nameof(model.Name), "Payment Method Name must be unique.");
			return View("PaymentMethodForm", model);
		}

		try
		{
			var method = await context.PaymentMethods.FirstOrDefaultAsync(pm => pm.Id == model.Id.Value);
			if (method == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Payment Method not found." });
				return RedirectToAction(nameof(PaymentMethods));
			}

			method.Update(model.Name, model.Status);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Payment Method updated successfully." });
			return RedirectToAction(nameof(PaymentMethods));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error updating payment method: " + ex.Message });
			return View("PaymentMethodForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> PaymentMethodDelete(int id)
	{
		try
		{
			var method = await context.PaymentMethods.FirstOrDefaultAsync(pm => pm.Id == id);
			if (method == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Payment Method not found." });
				return RedirectToAction(nameof(PaymentMethods));
			}

			context.PaymentMethods.Remove(method);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Payment Method deleted successfully." });
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting payment method: " + ex.Message });
		}

		return RedirectToAction(nameof(PaymentMethods));
	}
}
