using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Helpers;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
[Route("Medicines/Purchase/[action]")]
public class PurchaseController(
	ApplicationDbContext context,
	PurchaseService purchaseService,
	SupplierService supplierService
) : BaseController
{
	private const string FormView = "PurchaseForm";
	[HttpGet("/Medicines/Purchase")]
	[Authorize(Policy = Permissions.Purchase.List)]
	public IActionResult Index()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.List)]
	public async Task<IActionResult> Filter(CancellationToken ct = default)
	{
		var request = DataTableRequest.FromForm(Request.Form);

		var query = context.Purchases
			.Include(mp => mp.Supplier)
			.AsQueryable();

		var result = await DataTableHelper.FilterAsync(query, request, PurchaseColumns.GetColumnMap(), ct);

		var canView = User.HasPermission(Permissions.Purchase.View);
		var canEdit = User.HasPermission(Permissions.Purchase.Edit);
		var canDelete = User.HasPermission(Permissions.Purchase.Delete);

		var formattedData = PurchaseMapper.MapToPurchases(result.Data, canView, canEdit, canDelete);

		return Json(new
		{
			draw = request.Draw,
			recordsTotal = result.TotalRecords,
			recordsFiltered = result.FilteredRecords,
			data = formattedData
		});
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Purchase.View)]
	public async Task<IActionResult> View(int id, CancellationToken ct = default)
	{
		var result = await purchaseService.GetPurchaseAsync(id, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		var model = PurchaseMapper.MapToPurchaseView(result.Value);

		return View(model);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Purchase.Add)]
	public async Task<IActionResult> Add(CancellationToken ct = default)
	{
		var medicineViewModel = new PurchaseFormModel
		{
			Suppliers = await supplierService.GetSuppliersAsync(ct)
		};
		
		return View(FormView, medicineViewModel);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.Add)]
	public async Task<IActionResult> Add(PurchaseFormModel model, CancellationToken ct = default)
	{
		model.Suppliers = await supplierService.GetSuppliersAsync(ct);
		
		if (!ModelState.IsValid)
		{
			NotifyError("Please check your inputs.");
			return View(FormView, model);	
		}
		
		var result = await purchaseService.CreatePurchaseAsync(model, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model);

		NotifySuccess("Purchase created successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Purchase.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var result = await purchaseService.GetPurchaseAsync(id, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView);
		
		var suppliers = await supplierService.GetSuppliersAsync(ct);
		var model = PurchaseMapper.MapToPurchaseForm(result.Value, suppliers);

		return View(FormView, model);
	}

	[HttpPost("{id:int}")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.Edit)]
	public async Task<IActionResult> Edit(int id, PurchaseFormModel model, CancellationToken ct = default)
	{
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return BadRequest();
		}
		
		model.Suppliers = await supplierService.GetSuppliersAsync(ct);

		if (!ModelState.IsValid)
		{
			NotifyError("Please check your inputs.");
			return View(FormView, model);
		}

		var result = await purchaseService.UpdatePurchaseAsync(id, model, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model);

		NotifySuccess("Purchase updated successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		var result = await purchaseService.DeletePurchaseAsync(id, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		NotifySuccess("Purchase deleted successfully.");
		return RedirectToAction(nameof(Index));
	}

}
