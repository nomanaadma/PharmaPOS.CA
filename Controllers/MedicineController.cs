using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Models;
using System.Text.Json;
using PharmaPOS_MVC.Domain.Enums;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Medicines/[action]")]
public class MedicineController(
	ApplicationDbContext context
) : Controller
{
	[HttpGet]
	[Route("/Medicines")]
	public async Task<IActionResult> Index()
	{
		var medicines = await context.Medicines
			.OrderByDescending(m => m.CreatedDate)
			.Select(m => new MedicineViewModel
			{
				Id = m.Id,
				Name = m.Name,
				Company = m.Company,
				Generic = m.Generic,
				MedicineGroup = m.MedicineGroup,
				CategoryId = m.CategoryId,
				CategoryName = m.Category.Name,
				StoreBox = m.StoreBox,
				MinLevel = m.MinLevel,
				ReOrderLevel = m.ReOrderLevel,
				Unit = m.Unit,
				UnitPacking = m.UnitPacking,
				Note = m.Note,
				LiveStock = 0, // Default to 0 until batch/purchase is implemented
				CreatedDate = m.CreatedDate
			})
			.ToListAsync();

		return View(medicines);
	}

	[HttpGet]
	public async Task<IActionResult> Add()
	{
		var categoryItems = await GetMedicineCategoriesAsync();

		var medicineViewModel = new MedicineViewModel
		{
			Categories = categoryItems
		};
		
		return View("MedicineForm", medicineViewModel);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Add(MedicineViewModel model)
	{
		if (!ModelState.IsValid)
		{
			model.Categories = await GetMedicineCategoriesAsync();
			return View("MedicineForm", model);	
		}

		try
		{
			var medicine = new Medicine(
				model.Name,
				model.Company,
				model.Generic,
				model.MedicineGroup,
				model.CategoryId,
				model.StoreBox,
				model.MinLevel,
				model.ReOrderLevel,
				model.Unit,
				model.UnitPacking,
				model.Note
			);

			context.Medicines.Add(medicine);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Medicine created successfully." });
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error saving medicine: " + ex.Message });
			model.Categories = await GetMedicineCategoriesAsync();
			return View("MedicineForm", model);
		}
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> Edit(int id)
	{
		var medicine = await context.Medicines.FirstOrDefaultAsync(m => m.Id == id);
		if (medicine == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Medicine does not exist in database!" });
			return RedirectToAction(nameof(Index));
		}
		
		var medicineCategories =  await GetMedicineCategoriesAsync();

		var model = new MedicineViewModel
		{
			Id = medicine.Id,
			Name = medicine.Name,
			Company = medicine.Company,
			Generic = medicine.Generic,
			MedicineGroup = medicine.MedicineGroup,
			CategoryId = medicine.CategoryId,
			StoreBox = medicine.StoreBox,
			MinLevel = medicine.MinLevel,
			ReOrderLevel = medicine.ReOrderLevel,
			Unit = medicine.Unit,
			UnitPacking = medicine.UnitPacking,
			Note = medicine.Note,
			Categories = medicineCategories 
		};
		
		return View("MedicineForm", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(MedicineViewModel model)
	{
		if (!model.Id.HasValue)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Medicine ID is required for editing." });
			return RedirectToAction(nameof(Index));
		}

		if (!ModelState.IsValid)
		{
			model.Categories = await GetMedicineCategoriesAsync();
			return View("MedicineForm", model);
		}

		try
		{
			var medicine = await context.Medicines.AsTracking().FirstOrDefaultAsync(m => m.Id == model.Id.Value);
			if (medicine == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Medicine not found." });
				return RedirectToAction(nameof(Index));
			}

			medicine.Update(
				model.Name,
				model.Company,
				model.Generic,
				model.MedicineGroup,
				model.CategoryId,
				model.StoreBox,
				model.MinLevel,
				model.ReOrderLevel,
				model.Unit,
				model.UnitPacking,
				model.Note
			);

			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Medicine updated successfully." });
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error updating medicine: " + ex.Message });
			model.Categories = await GetMedicineCategoriesAsync();
			return View("MedicineForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var medicine = await context.Medicines.AsTracking().FirstOrDefaultAsync(m => m.Id == id);
			if (medicine == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Medicine not found." });
				return RedirectToAction(nameof(Index));
			}

			context.Medicines.Remove(medicine);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Medicine deleted successfully." });
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting medicine: " + ex.Message });
		}

		return RedirectToAction(nameof(Index));
	}

	private async Task<IEnumerable<SelectListItem>> GetMedicineCategoriesAsync()
	{
		return await context.MedicineCategories
			.OrderBy(c => c.Name)
			.Select(c => new SelectListItem
			{
				Value = c.Id.ToString(),
				Text = c.Name
			})
			.ToListAsync();
		
	}

	[HttpGet]
	[Route("/getmedicine")]
	public async Task<IActionResult> GetMedicine(string term)
	{
		if (string.IsNullOrWhiteSpace(term))
		{
			return Json(Array.Empty<object>());
		}

		var results = await context.Medicines
			.Where(m => EF.Functions.ILike(m.Name, $"%{term}%"))
			.Take(10)
			.Select(m => new
			{
				id = m.Id,
				label = m.Name,
				Generic = m.Generic
			})
			.ToListAsync();

		return Json(results);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Route("/getbatch")]
	public async Task<IActionResult> GetBatch(int id)
	{
		var currentMonth = DateTime.UtcNow.ToString("yyyyMM");

		var batches = await context.Batches
			.Where(b => b.MedicineId == id)
#pragma warning disable CA1845
			.Where(b => string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
#pragma warning restore CA1845
			.Where(b => (b.Qty - b.Sold) > 0)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.Expiry)
			.ToListAsync();
		
		if (!batches.Any())
		{
			return Json(new { error = true, message = "No active batches found for this medicine." });
		}

		var resultHtml = "<option value=\"\">Select Batch</option>";
		foreach (var batch in batches)
		{
			resultHtml += $"<option value=\"{batch.Id}\">{batch.BatchName}</option>";
		}

		return Json(new { error = false, result = resultHtml });
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Route("/getbatchdata")]
	public async Task<IActionResult> GetBatchData(int medicine, int batch)
	{
		var mBatch = await context.Batches
			.FirstOrDefaultAsync(b => b.MedicineId == medicine && b.Id == batch);

		if (mBatch == null)
		{
			return Json(new { error = true, message = "Batch not found." });
		}

		var result = new
		{
			expiry = mBatch.Expiry,
			available_quantity = mBatch.Qty - mBatch.Sold,
			saleprice = mBatch.SalePrice
		};

		return Json(new { error = false, result = result });
	}
}
