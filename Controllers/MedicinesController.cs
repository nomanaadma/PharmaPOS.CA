using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Models;
using System.Text.Json;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Domain.Enums;
using PharmaPOS_MVC.DTOs;
using PharmaPOS_MVC.Extensions;
using PharmaPOS_MVC.Mappers;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
public class MedicinesController(
	ApplicationDbContext context
) : BaseController
{
	[HttpGet]
	[Authorize(Policy = Permissions.Medicine.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var currentMonth = DateTime.UtcNow.ExpiryFormat();

		var medicines = await context.Medicines
			.Include(m => m.Category)
			.OrderByDescending(m => m.CreatedDate)
			.Select(m => new MedicineWithStockDto
			(
				Medicine: m, // The raw entity
				// The SQL aggregation
				LiveStock: m.Batches
					.Where(b => b.Status == Status.Active && string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
					.Sum(b => (decimal?)(b.Pricing.Qty - b.Sold)) ?? 0m
			))
			.ToListAsync(ct);

		var medicineResponse = MedicineMapper.MapToMedicinesWithStock(medicines);
		
		return View(medicineResponse);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Medicine.Add)]
	public async Task<IActionResult> Add(CancellationToken ct = default)
	{
		var medicineViewModel = new MedicineViewModel
		{
			Categories = await GetMedicineCategoriesAsync(ct)
		};
		
		return View("MedicineForm", medicineViewModel);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Medicine.Add)]
	
	public async Task<IActionResult> Add(MedicineViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			model.Categories = await GetMedicineCategoriesAsync(ct);
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
			await context.SaveChangesAsync(ct);

			NotifySuccess("Medicine created successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error saving medicine: " + ex.Message);
			model.Categories = await GetMedicineCategoriesAsync(ct);
			return View("MedicineForm", model);
		}
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Medicine.View)]
	
	public async Task<IActionResult> View(int id, CancellationToken ct = default)
	{
		var medicine = await context.Medicines
			.Include(m => m.Category)
			.FirstOrDefaultAsync(m => m.Id == id, ct);

		if (medicine == null)
		{
			NotifyWarning("Medicine does not exist in database!");
			return RedirectToAction(nameof(Index));
		}
		
		var today = DateTime.UtcNow.Date;
		var currentMonthDate = new DateTime(today.Year, today.Month, 1);
	
		var currentMonthStr = currentMonthDate.ExpiryFormat();

		var batchesQuery = await context.Batches
			.Where(b => b.MedicineId == id)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.MedicineId)
			.ToListAsync(ct);
		
		var liveBatches = batchesQuery.Where(b => 
				string.CompareOrdinal(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) >= 0);
		
		var badBatches = batchesQuery.Where(b => 
				string.CompareOrdinal(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) < 0)
			.Where(b => b.Pricing.Qty > b.Sold);
		
		var model = new MedicineViewPageModel
		{
			Medicine = MedicineMapper.MapToMedicine(medicine),
			LiveStock = StockMapper.MapToStocks(liveBatches),
			BadStock = StockMapper.MapToStocks(badBatches)
		};

		return View(model);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Medicine.Edit)]
	
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var medicine = await context.Medicines.FirstOrDefaultAsync(m => m.Id == id, ct);
		if (medicine == null)
		{
			NotifyWarning("Medicine does not exist in database!");
			return RedirectToAction(nameof(Index));
		}
		
		var medicineCategories =  await GetMedicineCategoriesAsync(ct);
		var model = MedicineMapper.MapToMedicine(medicine, medicineCategories);
		
		return View("MedicineForm", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Medicine.Edit)]
	
	public async Task<IActionResult> Edit(int id, MedicineViewModel model, CancellationToken ct = default)
	{
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return RedirectToAction(nameof(Index));
		}
		
		if (!model.Id.HasValue)
		{
			NotifyError("Medicine ID is required for editing.");
			return RedirectToAction(nameof(Index));
		}

		if (!ModelState.IsValid)
		{
			model.Categories = await GetMedicineCategoriesAsync(ct);
			return View("MedicineForm", model);
		}

		try
		{
			var medicine = await context.Medicines.FirstOrDefaultAsync(m => m.Id == model.Id.Value, ct);
			if (medicine == null)
			{
				NotifyError("Medicine not found.");
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

			await context.SaveChangesAsync(ct);

			NotifySuccess("Medicine updated successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error updating medicine: " + ex.Message);
			model.Categories = await GetMedicineCategoriesAsync(ct);
			return View("MedicineForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Medicine.Delete)]
	
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		try
		{
			var medicine = await context.Medicines.FirstOrDefaultAsync(m => m.Id == id, ct);
			if (medicine == null)
			{
				NotifyError("Medicine not found.");
				return RedirectToAction(nameof(Index));
			}

			context.Medicines.Remove(medicine);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Medicine deleted successfully.");
		}
		catch (Exception ex)
		{
			NotifyError("Error deleting medicine: " + ex.Message);
		}

		return RedirectToAction(nameof(Index));
	}

	private async Task<IEnumerable<SelectListItem>> GetMedicineCategoriesAsync(CancellationToken ct = default)
	{
		return await context.MedicineCategories
			.OrderBy(c => c.Name)
			.Select(c => new SelectListItem
			{
				Value = c.Id.ToString(),
				Text = c.Name
			})
			.ToListAsync(ct);
		
	}

	[HttpGet("/getmedicine")]
	public async Task<IActionResult> GetMedicine(string term, CancellationToken ct = default)
	{
		
		var canBillingAdd = User.HasClaim("Permission", Permissions.Billing.Add);
		var canBillingEdit = User.HasClaim("Permission", Permissions.Billing.Edit);
		var canPurchaseAdd = User.HasClaim("Permission", Permissions.Purchase.Add);
		var canPurchaseEdit = User.HasClaim("Permission", Permissions.Purchase.Edit);
		var canReportView = User.HasClaim("Permission", Permissions.Purchase.Edit);
		
		if (
			!canBillingAdd && 
			!canBillingEdit &&
			!canPurchaseAdd &&
			!canPurchaseEdit &&
			!canReportView
		)
		{
			return Forbid();
		}
		
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
			.ToListAsync(ct);

		return Json(results);
	}

	[HttpPost("/getbatch")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> GetBatch(int id, CancellationToken ct = default)
	{
		
		var canAdd = User.HasClaim("Permission", Permissions.Billing.Add);
		var canEdit = User.HasClaim("Permission", Permissions.Billing.Edit);
		
		if (!canAdd && !canEdit)
		{
			return Forbid();
		}
		
		var currentMonth = DateTime.UtcNow.ExpiryFormat();

		var batches = await context.Batches
			.Where(b => b.MedicineId == id)
			.Where(b => string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
			.Where(b => (b.Pricing.Qty - b.Sold) > 0)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.Expiry)
			.ToListAsync(ct);
		
		if (batches.Count == 0)
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

	[HttpPost("/getbatchdata")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> GetBatchData(int medicine, int batch, CancellationToken ct = default)
	{
		
		var canAdd = User.HasClaim("Permission", Permissions.Billing.Add);
		var canEdit = User.HasClaim("Permission", Permissions.Billing.Edit);
		
		if (!canAdd && !canEdit)
		{
			return Forbid();
		}
		
		var mBatch = await context.Batches
			.FirstOrDefaultAsync(b => b.MedicineId == medicine && b.Id == batch, ct);

		if (mBatch == null)
		{
			return Json(new { error = true, message = "Batch not found." });
		}

		var result = new
		{
			expiry = mBatch.Expiry,
			available_quantity = mBatch.Pricing.Qty - mBatch.Sold,
			saleprice = mBatch.Pricing.SalePrice
		};

		return Json(new { error = false, result = result });
	}
}
