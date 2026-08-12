using System.Threading;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Domain.Enums;
using PharmaPOS_MVC.Extensions;
using PharmaPOS_MVC.Models;
using PharmaPOS_MVC.Mappers;

namespace PharmaPOS_MVC.Controllers;

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
		var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault() ?? "1");
		var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
		var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
		var searchValue = Request.Form["search[value]"].FirstOrDefault();
		var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
		var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "asc";

		var query = context.Bills
			.AsQueryable();

		// Total count
		var totalRecords = await query.CountAsync(ct);

		// Apply search
		if (!string.IsNullOrWhiteSpace(searchValue))
		{
			decimal.TryParse(searchValue, out var searchDecimal);
			query = query.Where(mb => 
				EF.Functions.ILike(mb.Name, $"%{searchValue}%") ||
				(mb.Note != null && EF.Functions.ILike(mb.Note, $"%{searchValue}%")) ||
				(searchDecimal != 0 && (mb.Subtotal == searchDecimal || mb.Tax == searchDecimal || mb.DiscountValue == searchDecimal || mb.Amount == searchDecimal))
			);
		}

		// Apply SearchBuilder criteria
		int sbIndex = 0;
		while (Request.Form.ContainsKey($"searchBuilder[criteria][{sbIndex}][condition]"))
		{
			var origData = Request.Form[$"searchBuilder[criteria][{sbIndex}][origData]"].FirstOrDefault();
			var condition = Request.Form[$"searchBuilder[criteria][{sbIndex}][condition]"].FirstOrDefault();
			var type = Request.Form[$"searchBuilder[criteria][{sbIndex}][type]"].FirstOrDefault();
			var value0 = Request.Form[$"searchBuilder[criteria][{sbIndex}][value1]"].FirstOrDefault();
			var value1 = Request.Form[$"searchBuilder[criteria][{sbIndex}][value2]"].FirstOrDefault();

			if (!string.IsNullOrEmpty(origData) && !string.IsNullOrEmpty(condition))
			{
				if (origData == "name")
				{
					var val = value0 ?? "";
					query = condition switch
					{
						"=" => query.Where(mb => EF.Functions.ILike(mb.Name, val)),
						"!=" => query.Where(mb => !EF.Functions.ILike(mb.Name, val)),
						"starts" => query.Where(mb => EF.Functions.ILike(mb.Name, $"{val}%")),
						"!starts" => query.Where(mb => !EF.Functions.ILike(mb.Name, $"{val}%")),
						"contains" => query.Where(mb => EF.Functions.ILike(mb.Name, $"%{val}%")),
						"!contains" => query.Where(mb => !EF.Functions.ILike(mb.Name, $"%{val}%")),
						"ends" => query.Where(mb => EF.Functions.ILike(mb.Name, $"%{val}")),
						"!ends" => query.Where(mb => !EF.Functions.ILike(mb.Name, $"%{val}")),
						"null" => query.Where(mb => string.IsNullOrEmpty(mb.Name)),
						"!null" => query.Where(mb => !string.IsNullOrEmpty(mb.Name)),
						_ => query
					};
				}
				else if (origData == "subtotal" || origData == "tax" || origData == "discount_value" || origData == "amount")
				{
					decimal.TryParse(value0, out var d0);
					decimal.TryParse(value1, out var d1);

					if (origData == "subtotal")
					{
						query = condition switch
						{
							"=" => query.Where(mb => mb.Subtotal == d0),
							"!=" => query.Where(mb => mb.Subtotal != d0),
							"<" => query.Where(mb => mb.Subtotal < d0),
							"<=" => query.Where(mb => mb.Subtotal <= d0),
							">" => query.Where(mb => mb.Subtotal > d0),
							">=" => query.Where(mb => mb.Subtotal >= d0),
							"between" => query.Where(mb => mb.Subtotal >= d0 && mb.Subtotal <= d1),
							"!between" => query.Where(mb => mb.Subtotal < d0 || mb.Subtotal > d1),
							_ => query
						};
					}
					else if (origData == "tax")
					{
						query = condition switch
						{
							"=" => query.Where(mb => mb.Tax == d0),
							"!=" => query.Where(mb => mb.Tax != d0),
							"<" => query.Where(mb => mb.Tax < d0),
							"<=" => query.Where(mb => mb.Tax <= d0),
							">" => query.Where(mb => mb.Tax > d0),
							">=" => query.Where(mb => mb.Tax >= d0),
							"between" => query.Where(mb => mb.Tax >= d0 && mb.Tax <= d1),
							"!between" => query.Where(mb => mb.Tax < d0 || mb.Tax > d1),
							_ => query
						};
					}
					else if (origData == "discount_value")
					{
						query = condition switch
						{
							"=" => query.Where(mb => mb.DiscountValue == d0),
							"!=" => query.Where(mb => mb.DiscountValue != d0),
							"<" => query.Where(mb => mb.DiscountValue < d0),
							"<=" => query.Where(mb => mb.DiscountValue <= d0),
							">" => query.Where(mb => mb.DiscountValue > d0),
							">=" => query.Where(mb => mb.DiscountValue >= d0),
							"between" => query.Where(mb => mb.DiscountValue >= d0 && mb.DiscountValue <= d1),
							"!between" => query.Where(mb => mb.DiscountValue < d0 || mb.DiscountValue > d1),
							_ => query
						};
					}
					else if (origData == "amount")
					{
						query = condition switch
						{
							"=" => query.Where(mb => mb.Amount == d0),
							"!=" => query.Where(mb => mb.Amount != d0),
							"<" => query.Where(mb => mb.Amount < d0),
							"<=" => query.Where(mb => mb.Amount <= d0),
							">" => query.Where(mb => mb.Amount > d0),
							">=" => query.Where(mb => mb.Amount >= d0),
							"between" => query.Where(mb => mb.Amount >= d0 && mb.Amount <= d1),
							"!between" => query.Where(mb => mb.Amount < d0 || mb.Amount > d1),
							_ => query
						};
					}
				}
				else if (origData == "bill_date")
				{
					if (DateTime.TryParse(value0, out var dt0))
					{
						var nextDay = dt0.Date.AddDays(1);
						DateTime dt1;
						
						switch (condition)
						{
							case "=":
							case "contains":
								query = query.Where(mb => mb.BillDate >= dt0.Date && mb.BillDate < nextDay);
								break;
							case "!=":
							case "!contains":
								query = query.Where(mb => mb.BillDate < dt0.Date || mb.BillDate >= nextDay);
								break;
							case "<":
								query = query.Where(mb => mb.BillDate < dt0.Date);
								break;
							case "<=":
								query = query.Where(mb => mb.BillDate < nextDay);
								break;
							case ">":
								query = query.Where(mb => mb.BillDate >= nextDay);
								break;
							case ">=":
								query = query.Where(mb => mb.BillDate >= dt0.Date);
								break;
							case "between":
								if (DateTime.TryParse(value1, out dt1))
								{
									var endDay = dt1.Date.AddDays(1);
									query = query.Where(mb => mb.BillDate >= dt0.Date && mb.BillDate < endDay);
								}
								break;
							case "!between":
								if (DateTime.TryParse(value1, out dt1))
								{
									var endDay = dt1.Date.AddDays(1);
									query = query.Where(mb => mb.BillDate < dt0.Date || mb.BillDate >= endDay);
								}
								break;
						}
					}
				}
			}
			sbIndex++;
		}

		// Filtered count
		var filteredRecords = await query.CountAsync(ct);
		
		// Apply sorting
		if (!string.IsNullOrEmpty(sortColumnIndex))
		{
			switch (sortColumnIndex)
			{
				case "1":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mb => mb.Name) 
						: query.OrderBy(mb => mb.Name);
					break;
				case "2":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mb => mb.Subtotal) 
						: query.OrderBy(mb => mb.Subtotal);
					break;
				case "3":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mb => mb.Tax) 
						: query.OrderBy(mb => mb.Tax);
					break;
				case "4":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mb => mb.DiscountValue) 
						: query.OrderBy(mb => mb.DiscountValue);
					break;
				case "5":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mb => mb.Amount) 
						: query.OrderBy(mb => mb.Amount);
					break;
				case "6":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mb => mb.BillDate) 
						: query.OrderBy(mb => mb.BillDate);
					break;
				default:
					query = query.OrderByDescending(mb => mb.CreatedDate);
					break;
			}
		}
		else
		{
			query = query.OrderByDescending(mb => mb.Id);
		}

		// Pagination
		var pagedQuery = query.Skip(start);
		if (length > 0)
		{
			pagedQuery = pagedQuery.Take(length);
		}

		var rawData = await pagedQuery.ToListAsync(ct);
		
		var canView = User.HasPermission(Permissions.Billing.View);
		var canEdit = User.HasPermission(Permissions.Billing.Edit);
		var canDelete = User.HasPermission(Permissions.Billing.Delete);
		
		var formattedData = BillingMapper.MapToBills(rawData, canView, canEdit, canDelete);

		return Json(new
		{
			draw,
			recordsTotal = totalRecords,
			recordsFiltered = filteredRecords,
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
				
				var newCustomer = new Customer(
					firstName,
					lastName,
					model.Email ?? "no-email@example.com",
					model.Mobile ?? "0000000000",
					Gender.Male,
					Status.Active
				);
				
				context.Customers.Add(newCustomer);
				await context.SaveChangesAsync(ct);
				customerId = newCustomer.Id;
			}

			var bill = new Bill(
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

			context.Bills.Add(bill);
			await context.SaveChangesAsync(ct);

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
			return RedirectToAction(nameof(Index));
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
			return RedirectToAction(nameof(Index));
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

			if (bill == null) return NotFound();

			var customerId = model.CustomerId ?? bill.CustomerId;
			
			if (customerId == 0)
			{
				var nameParts = model.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				
				var lastName = nameParts[^1];
				
				var firstName = nameParts.Length > 1 
					? string.Join(" ", nameParts[..^1]) 
					: lastName;
				
				var newCustomer = new Customer(
					firstName,
					lastName,
					model.Email ?? "no-email@example.com",
					model.Mobile ?? "0000000000",
					Gender.Male,
					Status.Active
				);
				
				context.Customers.Add(newCustomer);
				await context.SaveChangesAsync(ct);
				customerId = newCustomer.Id;
			}

			bill.Update(customerId, model.Name, model.Email, model.Mobile, model.Method, model.BillDate.ToUniversalTime(), model.Subtotal, model.Tax, model.DiscountValue, model.Amount, model.Note);

			// Revert sold quantities for old items
			foreach (var oldItem in bill.Items)
			{
				var batch = await context.Batches
					
					.FirstOrDefaultAsync(mb => mb.Id == oldItem.BatchId && mb.MedicineId == oldItem.MedicineId, ct);
				
				batch?.UpdateSold(batch.Sold - oldItem.Qty);
			}

			context.BillItems.RemoveRange(bill.Items);
			await context.SaveChangesAsync(ct);

			// Add new items and update sold quantities
			foreach (var item in model.Items)
			{
				var billItem = new BillItem(bill.Id, item.MedicineId, item.Batch, item.Qty, item.Saleprice, item.Gross, item.Discounttype, item.Discount, item.Discountvalue, item.Tax, item.Taxprice, item.Price);
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
			if (bill == null) return RedirectToAction(nameof(Index));

			// Revert sold quantities
			foreach (var item in bill.Items)
			{
				var batch = await context.Batches
					
					.FirstOrDefaultAsync(mb => mb.Id == item.BatchId && mb.MedicineId == item.MedicineId, ct);
				
				batch?.UpdateSold(batch.Sold - item.Qty);
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

		if (bill == null) return NotFound();

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
