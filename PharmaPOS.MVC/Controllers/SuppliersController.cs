using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class SuppliersController(
	SupplierService supplierService,
	ILogger<SuppliersController> logger
) : BaseController
{
	[HttpGet]
	[Authorize(Policy = Permissions.Suppliers.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var suppliersQuery = await supplierService.GetAllSuppliersAsync(ct);
		var suppliers = SettingMapper.MapToSuppliers(suppliersQuery);
		return View(suppliers);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Suppliers.Add)]
	public IActionResult Add()
	{
		return View("SupplierForm", new SupplierViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Suppliers.Add)]
	public async Task<IActionResult> Add(SupplierViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return View("SupplierForm", model);
		}

		var result = await supplierService.CreateSupplierAsync(model, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, model);

		NotifySuccess("Supplier created successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Suppliers.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var result = await supplierService.GetSupplierAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors);

		var model = SettingMapper.MapToSupplier(result.Value);
		return View("SupplierForm", model);
	}
	
	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Suppliers.Edit)]
	public async Task<IActionResult> Edit(int id, SupplierViewModel model, CancellationToken ct = default)
	{
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return BadRequest();
		}
		
		if (model.Id == 0)
		{
			NotifyError("Supplier ID is required for editing.");
			return BadRequest();
		}

		if (!ModelState.IsValid)
		{
			return View("SupplierForm", model);
		}

		var result = await supplierService.UpdateSupplierAsync(id, model, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, model);

		NotifySuccess("Supplier updated successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Suppliers.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		var result = await supplierService.DeleteSupplierAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors);

		NotifySuccess("Supplier deleted successfully.");
		return RedirectToAction(nameof(Index));
	}

	private IActionResult HandleErrors(List<Error> errors, SupplierViewModel? model = null)
	{
		var firstError = errors[0];
		logger.LogWarning("Supplier operation failed: {ErrorCode} - {ErrorDescription}", firstError.Code, firstError.Description);

		switch (firstError.Type)
		{
			case ErrorType.NotFound:
				NotifyError(firstError.Description);
				return NotFound();
			case ErrorType.Validation:
			case ErrorType.Conflict:
				if (firstError.Code == "Supplier.EmailNotUnique" && model != null)
				{
					ModelState.AddModelError(nameof(model.Email), firstError.Description);
				}
				else
				{
					NotifyError(firstError.Description);
				}
				return model != null ? View("SupplierForm", model) : RedirectToAction(nameof(Index));
			default:
				NotifyError("An unexpected error occurred. Please try again.");
				return model != null ? View("SupplierForm", model) : RedirectToAction(nameof(Index));
		}
	}
}
