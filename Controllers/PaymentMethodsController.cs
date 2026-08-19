using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS.Domain.Entities;
using PharmaPOS_MVC.Models;
using System.Text.Json;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Mappers;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
public class PaymentMethodsController(
	ApplicationDbContext context
) : BaseController
{
	[HttpGet]
	[Authorize(Policy = Permissions.PaymentMethod.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var methodsQuery = await context.PaymentMethods
			.OrderBy(p => p.Id)
			.ToListAsync(ct);

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

		if (await context.PaymentMethods.AnyAsync(p => p.Name == model.Name, ct))
		{
			ModelState.AddModelError(nameof(model.Name), "Payment Method Name must be unique.");
			return View("PaymentMethodForm", model);
		}

		try
		{
			var method = new PaymentMethod(model.Name, model.Status);
			context.PaymentMethods.Add(method);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Payment method created successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error saving payment method: " + ex.Message);
			return View("PaymentMethodForm", model);
		}
	}

	[HttpGet]
	[Authorize(Policy = Permissions.PaymentMethod.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var method = await context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id, ct);
		if (method == null)
		{
			NotifyError("Payment method not found.");
			return NotFound();
		}

		var model = SettingMapper.MapToPaymentMethod(method);

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
		
		if (!model.Id.HasValue)
		{
			NotifyError("Payment Method ID is required for editing.");
			return BadRequest();
		}

		if (!ModelState.IsValid)
		{
			return View("PaymentMethodForm", model);
		}

		if (await context.PaymentMethods.AnyAsync(p => p.Name == model.Name && p.Id != model.Id.Value, ct))
		{
			ModelState.AddModelError(nameof(model.Name), "Payment Method Name must be unique.");
			return View("PaymentMethodForm", model);
		}

		try
		{
			var method = await context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == model.Id.Value, ct);
			if (method == null)
			{
				NotifyError("Payment method not found.");
				return NotFound();
			}

			method.Update(model.Name, model.Status);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Payment method updated successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error updating payment method: " + ex.Message);
			return View("PaymentMethodForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.PaymentMethod.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		try
		{
			var method = await context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id, ct);
			if (method == null)
			{
				NotifyError("Payment method not found.");
				return NotFound();
			}

			context.PaymentMethods.Remove(method);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Payment method deleted successfully.");
		}
		catch (Exception ex)
		{
			NotifyError("Error deleting payment method: " + ex.Message);
		}

		return RedirectToAction(nameof(Index));
	}
}
