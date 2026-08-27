using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Mappers;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class MedicinesController(
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
		
		var request = model.Adapt<MedicineRequest>();

		var result = await medicineService.CreateMedicineAsync(request, ct);

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
		var (liveBatches, badBatches) = await medicineService.GetMedicineStockSummaryAsync(id, ct);
		
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
		
		var request = model.Adapt<MedicineRequest>();

		var result = await medicineService.UpdateMedicineAsync(id, request, ct);

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

		var results = await medicineService.SearchMedicinesAsync(term, ct);

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
		
		var batches = await medicineService.GetActiveBatchesForMedicineAsync(id, ct);
		
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
		
		var mBatch = await medicineService.GetBatchDataAsync(medicine, batch, ct);

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

		return Json(new { error = false, result });
	}

}
