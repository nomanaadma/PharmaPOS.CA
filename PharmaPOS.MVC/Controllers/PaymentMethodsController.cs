using ErrorOr;
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
public class PaymentMethodsController(
	PaymentMethodService paymentMethodService
) : BaseController
{
	private const string FormView = "PaymentMethodForm";
	[HttpGet]
	[Authorize(Policy = Permissions.PaymentMethod.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var methodsQuery = await paymentMethodService.GetAllPaymentMethodsAsync(ct);
		var methods = SettingMapper.MapToPaymentMethods(methodsQuery);
		
		return View(methods);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.PaymentMethod.Add)]
	public IActionResult Add()
	{
		return View(FormView, new PaymentMethodViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.PaymentMethod.Add)]
	public async Task<IActionResult> Add(PaymentMethodViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return View(FormView, model);
		}
		
		var request = model.Adapt<PaymentMethodRequest>();

		var result = await paymentMethodService.CreatePaymentMethodAsync(request, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model, new() { ["PaymentMethod.NameNotUnique"] = "Name" });

		NotifySuccess("Payment method created successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	[Authorize(Policy = Permissions.PaymentMethod.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var result = await paymentMethodService.GetPaymentMethodAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		var model = SettingMapper.MapToPaymentMethod(result.Value);
		return View(FormView, model);
	}
	
	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.PaymentMethod.Edit)]
	public async Task<IActionResult> Edit(int id, PaymentMethodViewModel model, CancellationToken ct = default)
	{
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return BadRequest();
		}
		
		if (model.Id == 0)
		{
			NotifyError("Payment Method ID is required for editing.");
			return BadRequest();
		}

		if (!ModelState.IsValid)
		{
			return View(FormView, model);
		}
		
		var request = model.Adapt<PaymentMethodRequest>();

		var result = await paymentMethodService.UpdatePaymentMethodAsync(id, request, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView, model, new() { ["PaymentMethod.NameNotUnique"] = "Name" });

		NotifySuccess("Payment method updated successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.PaymentMethod.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		var result = await paymentMethodService.DeletePaymentMethodAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, FormView);

		NotifySuccess("Payment method deleted successfully.");
		return RedirectToAction(nameof(Index));
	}

}
