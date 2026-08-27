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
public class SuppliersController(
	SupplierService supplierService
) : BaseController
{
	private const string FormView = "SupplierForm";
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
		return View(FormView, new SupplierViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Suppliers.Add)]
	public async Task<IActionResult> Add(SupplierViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return View(FormView, model);
		}
		
		var request = model.Adapt<SupplierRequest>();

		var result = await supplierService.CreateSupplierAsync(request, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model, new() { ["Supplier.EmailNotUnique"] = "Email" });

		NotifySuccess("Supplier created successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Suppliers.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var result = await supplierService.GetSupplierAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		var model = SettingMapper.MapToSupplier(result.Value);
		return View(FormView, model);
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
			return View(FormView, model);
		}
		
		var request = model.Adapt<SupplierRequest>();

		var result = await supplierService.UpdateSupplierAsync(id, request, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model, new() { ["Supplier.EmailNotUnique"] = "Email" });

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
			return HandleErrors(result.Errors, FormView);

		NotifySuccess("Supplier deleted successfully.");
		return RedirectToAction(nameof(Index));
	}

}
