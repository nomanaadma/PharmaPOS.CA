using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Models;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class MedicinesController(
	ApplicationDbContext context,
	MedicineService medicineService,
	CategoryService categoryService
) : BaseController
{
	private const string FormView = "MedicineForm";
	[HttpGet]
	[Authorize(Policy = Permissions.Medicine.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var medicines = await medicineService.GetMedicinesWithStockAsync(ct);
		var medicineResponse = MedicineMapper.MapToMedicinesWithStock(medicines);
		
		return View(medicineResponse);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Medicine.Add)]
	public async Task<IActionResult> Add(CancellationToken ct = default)
	{
		var medicineViewModel = new MedicineViewModel
		{
			Categories = await categoryService.GetCategoriesAsync(ct)
		};
		
		return View(FormView, medicineViewModel);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Medicine.Add)]
	
	public async Task<IActionResult> Add(MedicineViewModel model, CancellationToken ct = default)
	{
		model.Categories = await categoryService.GetCategoriesAsync(ct);
		
		if (!ModelState.IsValid)
		{
			return View(FormView, model);	
		}

		var result = await medicineService.CreateMedicineAsync(model, ct);

		if (result.IsError)
		{
			return HandleErrors(result.Errors, FormView, model);
		}

		NotifySuccess("Medicine created successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Medicine.View)]
	
	public async Task<IActionResult> View(int id, CancellationToken ct = default)
	{
		var result = await medicineService.GetMedicineAsync(id, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		var medicine = result.Value;
		var today = DateTime.UtcNow.Date;
		var currentMonthStr = new DateTime(today.Year, today.Month, 1).ExpiryFormat();

		var batchesQuery = await context.Batches
			.Where(b => b.MedicineId == id)
			.Where(b => b.Status == Status.Active)
			.OrderBy(b => b.MedicineId).Include(batch => batch.Pricing)
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
		var result = await medicineService.GetMedicineAsync(id, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView);
		
		var medicineCategories = await categoryService.GetCategoriesAsync(ct);
		var model = MedicineMapper.MapToMedicine(result.Value, medicineCategories);
		
		return View(FormView, model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Medicine.Edit)]
	
	public async Task<IActionResult> Edit(int id, MedicineViewModel model, CancellationToken ct = default)
	{
		
		model.Categories = await categoryService.GetCategoriesAsync(ct);
		
			
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return BadRequest();
		}
		
		if (model.Id == 0)
		{
			NotifyError("Medicine ID is required for editing.");
			return BadRequest();
		}

		if (!ModelState.IsValid)
		{
			return View(FormView, model);
		}

		var result = await medicineService.UpdateMedicineAsync(id, model, ct);

		if (result.IsError)
		{
			return HandleErrors(result.Errors, FormView, model);
		}

		NotifySuccess("Medicine updated successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Medicine.Delete)]
	
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		var result = await medicineService.DeleteMedicineAsync(id, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		NotifySuccess("Medicine deleted successfully.");
		return RedirectToAction(nameof(Index));
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
		
		var mBatch = await context.Batches.Include(b => b.Pricing)
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
