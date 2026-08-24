using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Helpers;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
[Route("Medicines/Billing/[action]")]
public class BillingController(
	ApplicationDbContext context,
	BillingService billingService,
	ILogger<BillingController> logger
) : BaseController
{
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

		var query = context.Bills.AsQueryable();

		var result = await DataTableHelper.FilterAsync(query, request, BillColumns.GetColumnMap(), ct);

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
			PaymentMethods = await billingService.GetPaymentMethodsAsync(ct)
		};
		return View("BillingForm", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Billing.Add)]
	public async Task<IActionResult> Add(BillingFormModel model, CancellationToken ct = default)
	{
		model.PaymentMethods = await billingService.GetPaymentMethodsAsync(ct);
		
		if (!ModelState.IsValid)
		{
			NotifyError("Please check your inputs.");
			return View("BillingForm", model);
		}
		
		if (model.Items.Count == 0)
		{
			NotifyError("Missing Bill items.");
			return View("BillingForm", model);
		}

		try
		{
			var billId = await billingService.CreateBillAsync(model, ct);

			NotifySuccess("Bill created successfully.");
			return RedirectToAction(nameof(View), new { id = billId });
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to create bill");
			NotifyError("An unexpected error occurred while saving the bill. Please try again.");
			return View("BillingForm", model);
		}
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Billing.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var bill = await billingService.GetBillForEditAsync(id, ct);

		if (bill == null)
		{
			NotifyWarning("Bill does not exist in database!");
			return NotFound();
		}

		var paymentMethods = await billingService.GetPaymentMethodsAsync(ct);
		var model = BillingMapper.MapToBillingForm(bill, paymentMethods);

		return View("BillingForm", model);
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
		
		model.PaymentMethods = await billingService.GetPaymentMethodsAsync(ct);

		if (!ModelState.IsValid)
		{
			NotifyError("Please check your inputs.");
			return View("BillingForm", model);
		}
		
		if (model.Items.Count == 0)
		{
			NotifyError("Missing bill items.");
			return View("BillingForm", model);
		}

		try
		{
			var billId = await billingService.UpdateBillAsync(id, model, ct);

			NotifySuccess("Bill updated successfully.");
			return RedirectToAction(nameof(View), new { id = billId });
		}
		catch (KeyNotFoundException)
		{
			NotifyWarning("Bill does not exist in database!");
			return NotFound();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to update bill {BillId}", id);
			NotifyError("An unexpected error occurred while updating the bill. Please try again.");
			return View("BillingForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Billing.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		try
		{
			await billingService.DeleteBillAsync(id, ct);
			NotifySuccess("Bill deleted successfully.");
		}
		catch (KeyNotFoundException)
		{
			NotifyWarning("Bill does not exist in database!");
			return NotFound();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to delete bill {BillId}", id);
			NotifyError("An unexpected error occurred while deleting the bill. Please try again.");
		}

		return RedirectToAction(nameof(Index));
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Billing.View)]
	public async Task<IActionResult> View(int id, CancellationToken ct = default)
	{
		var bill = await billingService.GetBillForViewAsync(id, ct);

		if (bill == null)
		{
			NotifyWarning("Bill does not exist in database!");
			return NotFound();
		}

		var model = BillingMapper.MapToBillDetailsViewModel(bill);

		return View(model);
	}
}
