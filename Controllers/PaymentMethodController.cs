using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Models;
using System.Text.Json;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Mappers;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("PaymentMethods/[action]")]
public class PaymentMethodController(
	ApplicationDbContext context
) : Controller
{
	[HttpGet]
	[Route("/PaymentMethods")]
	[Authorize(Policy = Permissions.PaymentMethod.List)]
	public async Task<IActionResult> Index()
	{
		var methodsQuery = await context.PaymentMethods
			.OrderBy(p => p.Id)
			.ToListAsync();

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
	public async Task<IActionResult> Add(PaymentMethodViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View("PaymentMethodForm", model);
		}

		if (await context.PaymentMethods.AnyAsync(p => p.Name == model.Name))
		{
			ModelState.AddModelError(nameof(model.Name), "Payment Method Name must be unique.");
			return View("PaymentMethodForm", model);
		}

		try
		{
			var method = new PaymentMethod(model.Name, model.Status);
			context.PaymentMethods.Add(method);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Payment method created successfully." });
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error saving payment method: " + ex.Message });
			return View("PaymentMethodForm", model);
		}
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.PaymentMethod.Edit)]
	public async Task<IActionResult> Edit(int id)
	{
		var method = await context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id);
		if (method == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Payment method not found." });
			return RedirectToAction(nameof(Index));
		}

		var model = SettingMapper.MapToPaymentMethod(method);

		return View("PaymentMethodForm", model);
	}
	
	[HttpPost("{id:int}")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.PaymentMethod.Edit)]
	public async Task<IActionResult> Edit(int id, PaymentMethodViewModel model)
	{
		if (id != model.Id)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Bad Request: ID mismatch." });
			return RedirectToAction(nameof(Index));
		}
		
		if (!model.Id.HasValue)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Payment Method ID is required for editing." });
			return RedirectToAction(nameof(Index));
		}

		if (!ModelState.IsValid)
		{
			return View("PaymentMethodForm", model);
		}

		if (await context.PaymentMethods.AnyAsync(p => p.Name == model.Name && p.Id != model.Id.Value))
		{
			ModelState.AddModelError(nameof(model.Name), "Payment Method Name must be unique.");
			return View("PaymentMethodForm", model);
		}

		try
		{
			var method = await context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == model.Id.Value);
			if (method == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Payment method not found." });
				return RedirectToAction(nameof(Index));
			}

			method.Update(model.Name, model.Status);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Payment method updated successfully." });
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error updating payment method: " + ex.Message });
			return View("PaymentMethodForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.PaymentMethod.Delete)]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var method = await context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id);
			if (method == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Payment method not found." });
				return RedirectToAction(nameof(Index));
			}

			context.PaymentMethods.Remove(method);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Payment method deleted successfully." });
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting payment method: " + ex.Message });
		}

		return RedirectToAction(nameof(Index));
	}
}
