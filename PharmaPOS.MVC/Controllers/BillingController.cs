using System.Threading;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.MVC.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS.Domain.BillingAggregate;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.CustomerAggregate;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Helpers;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Mappers;

namespace PharmaPOS.MVC.Controllers;

[Authorize]
[Route("Medicines/Billing/[action]")]
public class BillingController(ApplicationDbContext context) : BaseController
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
			PaymentMethods = await GetPaymentMethodsAsync(ct)
		};
		return View("BillingForm", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Billing.Add)]
	public async Task<IActionResult> Add(BillingFormModel model, CancellationToken ct = default)
	{
		
		model.PaymentMethods = await GetPaymentMethodsAsync(ct);
		
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

		await using var transaction = await context.Database.BeginTransactionAsync(ct);
		try
		{
			var customerId = model.CustomerId ?? 0;
			if (customerId == 0)
			{
				var nameParts = model.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				
				var lastName = nameParts[^1];
				
				var firstName = nameParts.Length > 1 
					? string.Join(" ", nameParts[..^1]) 
					: lastName;
				
				var customerDetails = new CustomerDetails(
					firstName,
					lastName,
					model.Email ?? "no-email@example.com",
					model.Mobile ?? "0000000000",
					Gender.Male,
					Status.Active
				);
				var newCustomer = new Customer(customerDetails);
				
				context.Customers.Add(newCustomer);
				await context.SaveChangesAsync(ct);
				customerId = newCustomer.Id;
			}

			var billDetails = new BillDetails(
				customerId,
				model.Name,
				model.Email,
				model.Mobile,
				model.Method,
				model.Date.ToUniversalTime(),
				model.SubTotal,
				model.Tax,
				model.Discount,
				model.Amount,
				model.Note
			);
			var bill = new Bill(billDetails);

			context.Bills.Add(bill);
			await context.SaveChangesAsync(ct);

			foreach (var item in model.Items)
			{
				
				var pricing = PricingHelper.PricingDetails(item);

				var billItemDetails = new BillItemDetails(
					bill.Id,
					item.MedicineId,
					item.Batch,
					pricing
				);
				var billItem = new BillItem(billItemDetails);
				context.BillItems.Add(billItem);

				var batch = await context.Batches
					
					.FirstOrDefaultAsync(mb => mb.Id == item.Batch && mb.MedicineId == item.MedicineId, ct);
			
				batch?.UpdateSold(batch.Sold + item.Qty);
			}

			await context.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);

			NotifySuccess("Bill created successfully.");
			return RedirectToAction(nameof(View), new { id = bill.Id });
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(ct);
			NotifyError("Error saving bill: " + ex.Message);
			return View("BillingForm", model);
		}
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Billing.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var currentMonth = DateTime.UtcNow.ExpiryFormat();
		
		var bill = await context.Bills
			.Include(mb => mb.Items)
				.ThenInclude(bi => bi.Medicine)
					.ThenInclude(m => m.Batches
						.Where(b => string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
						.Where(b => b.Status == Status.Active))
			.Include(mb => mb.Items)
				.ThenInclude(bi => bi.Batch)
			.AsSplitQuery()
			.FirstOrDefaultAsync(mb => mb.Id == id, ct);

		if (bill == null)
		{
			NotifyWarning("Bill does not exist in database!");
			return NotFound();
		}

		foreach (var item in bill.Items)
		{
			var batchAlreadyLoaded = item.Medicine.Batches.Any(b => b.Id == item.BatchId);
		
			if (batchAlreadyLoaded) continue;
			
			var activeBatch = await context.Batches.FirstOrDefaultAsync(b => b.Id == item.BatchId, ct);
		
			if (activeBatch != null)
			{
				item.Medicine.Batches.Add(activeBatch);
			}
		
		}
		
		var paymentMethods = await GetPaymentMethodsAsync(ct);
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
		
		model.PaymentMethods = await GetPaymentMethodsAsync(ct);

		if (!ModelState.IsValid)
		{
			NotifyError("Please check your inputs.");
			return View("BillingForm", model);
		}
		
		if (model.Items.Count == 0)
		{
			NotifyError("Missing purchase items.");
			return View("BillingForm", model);
		}

		await using var transaction = await context.Database.BeginTransactionAsync(ct);
		try
		{
			var bill = await context.Bills
				.Include(mb => mb.Items)
				
				.FirstOrDefaultAsync(mb => mb.Id == id, ct);

			if (bill == null)
			{
				NotifyWarning("Bill does not exist in database!");
				return NotFound();
			}

			var customerId = model.CustomerId ?? bill.CustomerId;
			
			if (customerId == 0)
			{
				var nameParts = model.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				
				var lastName = nameParts[^1];
				
				var firstName = nameParts.Length > 1 
					? string.Join(" ", nameParts[..^1]) 
					: lastName;
				
				var customerDetails = new CustomerDetails(
					firstName,
					lastName,
					model.Email ?? "no-email@example.com",
					model.Mobile ?? "0000000000",
					Gender.Male,
					Status.Active
				);
				var newCustomer = new Customer(customerDetails);
				
				context.Customers.Add(newCustomer);
				await context.SaveChangesAsync(ct);
				customerId = newCustomer.Id;
			}

			var billDetails = new BillDetails(
				customerId,
				model.Name,
				model.Email,
				model.Mobile,
				model.Method,
				model.Date.ToUniversalTime(),
				model.SubTotal,
				model.Tax,
				model.Discount,
				model.Amount,
				model.Note
			);
			bill.Update(billDetails);

			// Revert sold quantities for old items
			foreach (var oldItem in bill.Items)
			{
				var batch = await context.Batches
					.FirstOrDefaultAsync(mb => mb.Id == oldItem.BatchId && mb.MedicineId == oldItem.MedicineId, ct);
				
				batch?.UpdateSold(batch.Sold - oldItem.Pricing.Qty);
			}

			context.BillItems.RemoveRange(bill.Items);
			await context.SaveChangesAsync(ct);

			// Add new items and update sold quantities
			foreach (var item in model.Items)
			{

				var pricing = PricingHelper.PricingDetails(item);
				
				var billItemDetails = new BillItemDetails(bill.Id, item.MedicineId, item.Batch, pricing);
				var billItem = new BillItem(billItemDetails);
				context.BillItems.Add(billItem);
				
				var batch = await context.Batches
					
					.FirstOrDefaultAsync(mb => mb.Id == item.Batch && mb.MedicineId == item.MedicineId, ct);

				batch?.UpdateSold(batch.Sold + item.Qty);
			}

			await context.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);

			NotifySuccess("Bill updated successfully.");
			return RedirectToAction(nameof(View), new { id = bill.Id });
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(ct);
			NotifyError("Error updating bill: " + ex.Message);
			return View("BillingForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Billing.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		await using var transaction = await context.Database.BeginTransactionAsync(ct);
		try
		{
			var bill = await context.Bills.Include(mb => mb.Items).FirstOrDefaultAsync(mb => mb.Id == id, ct);
			
			if (bill == null)
			{
				NotifyWarning("Bill does not exist in database!");
				return NotFound();
			}
			
			// Revert sold quantities
			foreach (var item in bill.Items)
			{
				var batch = await context.Batches
					
					.FirstOrDefaultAsync(mb => mb.Id == item.BatchId && mb.MedicineId == item.MedicineId, ct);
				
				batch?.UpdateSold(batch.Sold - item.Pricing.Qty);
			}

			context.Bills.Remove(bill);
			await context.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);

			NotifySuccess("Bill deleted successfully.");
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(ct);
			NotifyError("Error deleting bill: " + ex.Message);
		}

		return RedirectToAction(nameof(Index));
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Billing.View)]
	public async Task<IActionResult> View(int id, CancellationToken ct = default)
	{
		var bill = await context.Bills
			.Include(mb => mb.PaymentMethod)
			.Include(mb => mb.Items)
				.ThenInclude(bi => bi.Medicine)
			.Include(mb => mb.Items)
				.ThenInclude(bi => bi.Batch)
			.AsSplitQuery()
			.FirstOrDefaultAsync(mb => mb.Id == id, ct);

		if (bill == null)
		{
			NotifyWarning("Bill does not exist in database!");
			return NotFound();
		}

		var model = BillingMapper.MapToBillDetailsViewModel(bill);

		return View(model);
	}

	private async Task<IEnumerable<SelectListItem>> GetPaymentMethodsAsync(CancellationToken ct = default)
	{
		return await context.PaymentMethods
			.Where(pm => pm.Status == Status.Active)
			.Select(pm => 
				new SelectListItem
				{
					Value = pm.Id.ToString(), 
					Text = pm.Name
				})
			.ToListAsync(ct);
	}
}
