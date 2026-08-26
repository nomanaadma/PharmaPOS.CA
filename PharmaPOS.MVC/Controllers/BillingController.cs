using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Services;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Helpers;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
[Route("Medicines/Billing/[action]")]
public class BillingController(
	BillingService billingService,
	PaymentMethodService paymentMethodService
) : BaseController
{
	private const string FormView = "BillingForm";
	[HttpGet("/Medicines/Billing")]
	[Authorize(Policy = Permissions.Billing.List)]
	public IActionResult Index()
	{
		ViewData["DeleteMsg"] = "Inventory will be updated and those Stock will be added in Inventory.";
		return View();
	}
	
	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Billing.List)]
	public async Task<IActionResult> Filter(CancellationToken ct = default)
	{
		var request = DataTableRequest.FromForm(Request.Form);

		var result = await billingService.GetFilteredBillsAsync(request, BillColumns.GetColumnMap(), ct);

		var canView = User.HasPermission(Permissions.Billing.View);
		var canEdit = User.HasPermission(Permissions.Billing.Edit);
		var canDelete = User.HasPermission(Permissions.Billing.Delete);

		var formattedData = BillingMapper.MapToBills(result.Data, canView, canEdit, canDelete);

		return Json(new
		{
			draw = request.Draw,
			recordsTotal = result.TotalRecords,
			recordsFiltered = result.FilteredRecords,
			data = formattedData
		});
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Billing.Add)]
	public async Task<IActionResult> Add(CancellationToken ct = default)
	{
		var model = new BillingFormModel
		{
			PaymentMethods = await paymentMethodService.GetPaymentMethodsAsync(ct)
		};
		return View(FormView, model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Billing.Add)]
	public async Task<IActionResult> Add(BillingFormModel model, CancellationToken ct = default)
	{
		model.PaymentMethods = await paymentMethodService.GetPaymentMethodsAsync(ct);
		
		if (!ModelState.IsValid)
		{
			NotifyError("Please check your inputs.");
			return View(FormView, model);
		}

		var result = await billingService.CreateBillAsync(model, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model);

		NotifySuccess("Bill created successfully.");
		return RedirectToAction(nameof(View), new { id = result.Value });
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Billing.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var result = await billingService.GetBillForEditAsync(id, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		var paymentMethods = await paymentMethodService.GetPaymentMethodsAsync(ct);
		var model = BillingMapper.MapToBillingForm(result.Value, paymentMethods);

		return View(FormView, model);
	}

	[HttpPost("{id:int}")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Billing.Edit)]
	public async Task<IActionResult> Edit(int id, BillingFormModel model, CancellationToken ct = default)
	{
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return BadRequest();
		}
		
		model.PaymentMethods = await paymentMethodService.GetPaymentMethodsAsync(ct);

		if (!ModelState.IsValid)
		{
			NotifyError("Please check your inputs.");
			return View(FormView, model);
		}

		var result = await billingService.UpdateBillAsync(id, model, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model);

		NotifySuccess("Bill updated successfully.");
		return RedirectToAction(nameof(View), new { id = result.Value });
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Billing.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		var result = await billingService.DeleteBillAsync(id, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		NotifySuccess("Bill deleted successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Billing.View)]
	public async Task<IActionResult> View(int id, CancellationToken ct = default)
	{
		var result = await billingService.GetBillAsync(id, ct);

		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		var model = BillingMapper.MapToBillDetailsViewModel(result.Value);

		return View(model);
	}

}
