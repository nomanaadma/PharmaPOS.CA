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
		var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
		var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "asc";

		var query = context.MedicineBills
			.AsQueryable();

		// Total count
		var totalRecords = await query.CountAsync();

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
		var filteredRecords = await query.CountAsync();
		
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

		var rawData = await pagedQuery.ToListAsync();

		var formattedData = rawData.Select(mb => new
		{
			id = mb.Id,
			name = mb.Name,
			subtotal = "Rs " + mb.Subtotal.ToString("F2"),
			tax = "Rs " + mb.Tax.ToString("F2"),
			discount_value = "Rs " + mb.DiscountValue.ToString("F2"),
			amount = "Rs " + mb.Amount.ToString("F2"),
			bill_date = mb.BillDate.ToString("yyyy-MM-dd"),
			action = $@"
				<div class=""dropdown d-inline-block"">
					<a class=""text-primary edit dropdown-toggle"" data-toggle=""dropdown""><i class=""las la-ellipsis-h""></i></a>
					<ul class=""dropdown-menu dropdown-menu-right"">
						<li><a href=""/Medicines/Billing/View/{mb.Id}""><i class=""las la-laptop pr-2""></i>View</a></li>
						<li><a href=""/Medicines/Billing/Edit/{mb.Id}""><i class=""las la-edit pr-2""></i>Edit</a></li>
					</ul>
				</div>
				<a class=""table-delete text-danger delete"" data-toggle=""tooltip"" title=""Delete"">
					<i class=""las la-trash-alt""></i>
					<input type=""hidden"" value=""{mb.Id}"">
				</a>"
		}).ToList();

		return Json(new
		{
			draw,
			recordsTotal = totalRecords,
			recordsFiltered = filteredRecords,
			data = formattedData
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
	[Route("{id:int}")]
	public async Task<IActionResult> Edit(int id)
	{
		var bill = await context.MedicineBills
			.Include(mb => mb.Items)
				.ThenInclude(bi => bi.Medicine)
					.ThenInclude(m => m.Batches)
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
				Price = bi.Price,
				Batches = bi.Medicine.Batches.Select(mb => new BillingItemBatchesFormModel
					{
						Id = mb.Id,
						Qty = mb.Qty,
						Batch = mb.Batch,
						Sold = mb.Sold
					}).ToList()
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
		return await context.PaymentMethods
			.Where(pm => pm.Status == Status.Active)
			.Select(pm => 
				new SelectListItem
				{
					Value = pm.Id.ToString(), 
					Text = pm.Name
				})
			.ToListAsync();
	}
}
