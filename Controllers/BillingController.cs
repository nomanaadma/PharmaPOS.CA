using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Domain.Enums;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Medicines/Billing/[action]")]
public class BillingController(ApplicationDbContext context) : Controller
{
	[HttpGet]
	[Route("/Medicines/Billing")]
	public IActionResult Index()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Filter()
	{
		var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault() ?? "1");
		var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
		var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
		var searchValue = Request.Form["search[value]"].FirstOrDefault();

		var query = context.MedicineBills
			.Include(mb => mb.Customer)
			.AsQueryable();

		var totalRecords = await query.CountAsync();

		if (!string.IsNullOrWhiteSpace(searchValue))
		{
			decimal.TryParse(searchValue, out var searchDecimal);
			query = query.Where(mb => 
				EF.Functions.ILike(mb.Name, $"%{searchValue}%") ||
				(searchDecimal != 0 && (mb.Subtotal == searchDecimal || mb.Tax == searchDecimal || mb.DiscountValue == searchDecimal || mb.Amount == searchDecimal))
			);
		}

		var filteredRecords = await query.CountAsync();

		query = query.OrderByDescending(mb => mb.Id).Skip(start).Take(length);

		var data = await query.Select(mb => new
		{
			id = mb.Id,
			name = mb.Name,
			subtotal = mb.Subtotal,
			tax = mb.Tax,
			discount_value = mb.DiscountValue,
			amount = mb.Amount,
			bill_date = mb.BillDate.ToString("yyyy-MM-dd")
		}).ToListAsync();

		return Json(new
		{
			draw,
			recordsTotal = totalRecords,
			recordsFiltered = filteredRecords,
			data
		});
	}

	[HttpGet]
	public async Task<IActionResult> Add()
	{
		var model = new BillingFormModel
		{
			PaymentMethods = await GetPaymentMethodsAsync()
		};
		return View("BillingForm", model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Add([Bind(Prefix = "billing")] BillingFormModel model)
	{
		if (!ModelState.IsValid)
		{
			model.PaymentMethods = await GetPaymentMethodsAsync();
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Please check your inputs." });
			return View("BillingForm", model);
		}

		using var transaction = await context.Database.BeginTransactionAsync();
		try
		{
			int customerId = model.CustomerId ?? 0;
			if (customerId == 0 && !string.IsNullOrWhiteSpace(model.Name))
			{
				var newCustomer = new Customer(
					model.Name,
					".", // Placeholder since we only get full name in Name field
					model.Email ?? "no-email@example.com",
					model.Mobile ?? "0000000000",
					Gender.Male,
					Status.Active
				);
				context.Customers.Add(newCustomer);
				await context.SaveChangesAsync();
				customerId = newCustomer.Id;
			}

			var bill = new MedicineBill(
				customerId,
				model.Name,
				model.Email,
				model.Mobile,
				model.Method,
				model.BillDate.ToUniversalTime(),
				model.Subtotal,
				model.Tax,
				model.DiscountValue,
				model.Amount,
				model.Note
			);

			context.MedicineBills.Add(bill);
			await context.SaveChangesAsync();

			foreach (var item in model.Items)
			{
				var billItem = new BillItem(
					bill.Id,
					item.MedicineId,
					item.Batch,
					item.Qty,
					item.Saleprice,
					item.Gross,
					item.Discounttype,
					item.Discount,
					item.Discountvalue,
					item.Tax,
					item.Taxprice,
					item.Price
				);
				context.BillItems.Add(billItem);

				var batch = await context.MedicineBatches.FindAsync(item.Batch);
				if (batch != null)
				{
					batch.UpdateSold(batch.Sold + item.Qty);
				}
			}

			await context.SaveChangesAsync();
			await transaction.CommitAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Bill created successfully." });
			return RedirectToAction(nameof(View), new { id = bill.Id });
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			model.PaymentMethods = await GetPaymentMethodsAsync();
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error saving bill: " + ex.Message });
			return View("BillingForm", model);
		}
	}

	[HttpGet]
	[Route("{id}")]
	public async Task<IActionResult> Edit(int id)
	{
		var bill = await context.MedicineBills
			.Include(mb => mb.Items)
				.ThenInclude(bi => bi.Medicine)
			.Include(mb => mb.Items)
				.ThenInclude(bi => bi.Batch)
			.FirstOrDefaultAsync(mb => mb.Id == id);

		if (bill == null)
		{
			return RedirectToAction(nameof(Index));
		}

		var model = new BillingFormModel
		{
			Id = bill.Id,
			CustomerId = bill.CustomerId,
			Name = bill.Name,
			Email = bill.Email,
			Mobile = bill.Mobile,
			Method = bill.MethodId,
			BillDate = bill.BillDate,
			Subtotal = bill.Subtotal,
			Tax = bill.Tax,
			DiscountValue = bill.DiscountValue,
			Amount = bill.Amount,
			Note = bill.Note,
			PaymentMethods = await GetPaymentMethodsAsync(),
			Items = bill.Items.Select(bi => new BillingItemFormModel
			{
				Id = bi.Id,
				MedicineId = bi.MedicineId,
				Name = bi.Medicine.Name,
				Batch = bi.BatchId,
				BatchName = bi.Batch.Batch,
				Expiry = bi.Batch.Expiry,
				Qty = bi.Qty,
				Saleprice = bi.SalePrice,
				Gross = bi.Gross,
				Discounttype = bi.DiscountType,
				Discount = bi.Discount,
				Discountvalue = bi.DiscountValue,
				Tax = bi.Tax,
				Taxprice = bi.TaxPrice,
				Price = bi.Price
			}).ToList()
		};

		return View("BillingForm", model);
	}

	[HttpPost]
	[Route("{id}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, [Bind(Prefix = "billing")] BillingFormModel model)
	{
		if (id != model.Id) return NotFound();

		if (!ModelState.IsValid)
		{
			model.PaymentMethods = await GetPaymentMethodsAsync();
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Please check your inputs." });
			return View("BillingForm", model);
		}

		using var transaction = await context.Database.BeginTransactionAsync();
		try
		{
			var bill = await context.MedicineBills
				.Include(mb => mb.Items)
				.FirstOrDefaultAsync(mb => mb.Id == id);

			if (bill == null) return NotFound();

			int customerId = model.CustomerId ?? bill.CustomerId;
			if (customerId == 0 && !string.IsNullOrWhiteSpace(model.Name))
			{
				var newCustomer = new Customer(
					model.Name, ".", model.Email ?? "no-email@example.com", model.Mobile ?? "0000000000", Gender.Male, Status.Active
				);
				context.Customers.Add(newCustomer);
				await context.SaveChangesAsync();
				customerId = newCustomer.Id;
			}

			bill.Update(customerId, model.Name, model.Email, model.Mobile, model.Method, model.BillDate.ToUniversalTime(), model.Subtotal, model.Tax, model.DiscountValue, model.Amount, model.Note);

			// Revert sold quantities for old items
			foreach (var oldItem in bill.Items)
			{
				var batch = await context.MedicineBatches.FindAsync(oldItem.BatchId);
				if (batch != null) batch.UpdateSold(batch.Sold - oldItem.Qty);
			}

			context.BillItems.RemoveRange(bill.Items);
			await context.SaveChangesAsync();

			// Add new items and update sold quantities
			foreach (var item in model.Items)
			{
				var billItem = new BillItem(bill.Id, item.MedicineId, item.Batch, item.Qty, item.Saleprice, item.Gross, item.Discounttype, item.Discount, item.Discountvalue, item.Tax, item.Taxprice, item.Price);
				context.BillItems.Add(billItem);

				var batch = await context.MedicineBatches.FindAsync(item.Batch);
				if (batch != null) batch.UpdateSold(batch.Sold + item.Qty);
			}

			await context.SaveChangesAsync();
			await transaction.CommitAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Bill updated successfully." });
			return RedirectToAction(nameof(View), new { id = bill.Id });
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			model.PaymentMethods = await GetPaymentMethodsAsync();
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error updating bill: " + ex.Message });
			return View("BillingForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(int id)
	{
		using var transaction = await context.Database.BeginTransactionAsync();
		try
		{
			var bill = await context.MedicineBills.Include(mb => mb.Items).FirstOrDefaultAsync(mb => mb.Id == id);
			if (bill == null) return RedirectToAction(nameof(Index));

			// Revert sold quantities
			foreach (var item in bill.Items)
			{
				var batch = await context.MedicineBatches.FindAsync(item.BatchId);
				if (batch != null) batch.UpdateSold(batch.Sold - item.Qty);
			}

			context.MedicineBills.Remove(bill);
			await context.SaveChangesAsync();
			await transaction.CommitAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Bill deleted successfully." });
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting bill: " + ex.Message });
		}

		return RedirectToAction(nameof(Index));
	}

	[HttpGet]
	[Route("{id}")]
	public async Task<IActionResult> View(int id)
	{
		var bill = await context.MedicineBills
			.Include(mb => mb.Customer)
			.Include(mb => mb.PaymentMethod)
			.Include(mb => mb.Items)
				.ThenInclude(bi => bi.Medicine)
			.Include(mb => mb.Items)
				.ThenInclude(bi => bi.Batch)
			.FirstOrDefaultAsync(mb => mb.Id == id);

		if (bill == null) return NotFound();

		return View(bill);
	}

	private async Task<IEnumerable<SelectListItem>> GetPaymentMethodsAsync()
	{
		return await context.PaymentMethods.Where(pm => pm.Status == Status.Active).OrderBy(pm => pm.Name)
			.Select(pm => new SelectListItem { Value = pm.Id.ToString(), Text = pm.Name })
			.ToListAsync();
	}
}
