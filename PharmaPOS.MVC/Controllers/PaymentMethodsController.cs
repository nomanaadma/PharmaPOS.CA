using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
public class PaymentMethodsController(
	PaymentMethodService paymentMethodService,
	ILogger<PaymentMethodsController> logger
) : BaseController
{
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
		return View("PaymentMethodForm", new PaymentMethodViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.PaymentMethod.Add)]
	public async Task<IActionResult> Add(PaymentMethodViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return View("PaymentMethodForm", model);
		}

		var result = await paymentMethodService.CreatePaymentMethodAsync(model, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, model);

		NotifySuccess("Payment method created successfully.");
		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	[Authorize(Policy = Permissions.PaymentMethod.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var result = await paymentMethodService.GetPaymentMethodAsync(id, ct);
		if (result.IsError)
			return HandleErrors(result.Errors);

		var model = SettingMapper.MapToPaymentMethod(result.Value);
		return View("PaymentMethodForm", model);
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
			return View("PaymentMethodForm", model);
		}

		var result = await paymentMethodService.UpdatePaymentMethodAsync(id, model, ct);
		if (result.IsError)
			return HandleErrors(result.Errors, model);

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
			return HandleErrors(result.Errors);

		NotifySuccess("Payment method deleted successfully.");
		return RedirectToAction(nameof(Index));
	}

	private IActionResult HandleErrors(List<Error> errors, PaymentMethodViewModel? model = null)
	{
		var firstError = errors[0];
		logger.LogWarning("PaymentMethod operation failed: {ErrorCode} - {ErrorDescription}", firstError.Code, firstError.Description);

		switch (firstError.Type)
		{
			case ErrorType.NotFound:
				NotifyError(firstError.Description);
				return NotFound();
			case ErrorType.Validation:
			case ErrorType.Conflict:
				if (firstError.Code == "PaymentMethod.NameNotUnique" && model != null)
				{
					ModelState.AddModelError(nameof(model.Name), firstError.Description);
				}
				else
				{
					NotifyError(firstError.Description);
				}
				return model != null ? View("PaymentMethodForm", model) : RedirectToAction(nameof(Index));
			default:
				NotifyError("An unexpected error occurred. Please try again.");
				return model != null ? View("PaymentMethodForm", model) : RedirectToAction(nameof(Index));
		}
	}
}
