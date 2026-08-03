using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Extensions;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Medicines/Purchase/[action]")]
public class PurchaseController(
	ApplicationDbContext context
) : Controller
{
	[HttpGet]
	[Route("/Medicines/Purchase")]
	[Authorize(Policy = Permissions.Purchase.List)]
	public IActionResult Index()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.List)]
	public async Task<IActionResult> Filter()
	{
		var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault() ?? "1");
		var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
		var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
		var searchValue = Request.Form["search[value]"].FirstOrDefault();
		var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
		var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "asc";

		var query = context.Purchases
			.Include(mp => mp.Supplier)
			.AsQueryable();

		// Total count
		var totalRecords = await query.CountAsync();

		// Apply search
		if (!string.IsNullOrWhiteSpace(searchValue))
		{
			decimal.TryParse(searchValue, out var searchDecimal);
			query = query.Where(mp => 
				EF.Functions.ILike(mp.Supplier.Name, $"%{searchValue}%") ||
				(mp.Note != null && EF.Functions.ILike(mp.Note, $"%{searchValue}%")) ||
				(searchDecimal != 0 && (mp.Total == searchDecimal || mp.Tax == searchDecimal || mp.DiscountValue == searchDecimal || mp.Amount == searchDecimal))
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
						"=" => query.Where(mp => EF.Functions.ILike(mp.Supplier.Name, val)),
						"!=" => query.Where(mp => !EF.Functions.ILike(mp.Supplier.Name, val)),
						"starts" => query.Where(mp => EF.Functions.ILike(mp.Supplier.Name, $"{val}%")),
						"!starts" => query.Where(mp => !EF.Functions.ILike(mp.Supplier.Name, $"{val}%")),
						"contains" => query.Where(mp => EF.Functions.ILike(mp.Supplier.Name, $"%{val}%")),
						"!contains" => query.Where(mp => !EF.Functions.ILike(mp.Supplier.Name, $"%{val}%")),
						"ends" => query.Where(mp => EF.Functions.ILike(mp.Supplier.Name, $"%{val}")),
						"!ends" => query.Where(mp => !EF.Functions.ILike(mp.Supplier.Name, $"%{val}")),
						"null" => query.Where(mp => string.IsNullOrEmpty(mp.Supplier.Name)),
						"!null" => query.Where(mp => !string.IsNullOrEmpty(mp.Supplier.Name)),
						_ => query
					};
				}
				else if (origData == "total" || origData == "tax" || origData == "discount_value" || origData == "amount")
				{
					decimal.TryParse(value0, out var d0);
					decimal.TryParse(value1, out var d1);

					if (origData == "total")
					{
						query = condition switch
						{
							"=" => query.Where(mp => mp.Total == d0),
							"!=" => query.Where(mp => mp.Total != d0),
							"<" => query.Where(mp => mp.Total < d0),
							"<=" => query.Where(mp => mp.Total <= d0),
							">" => query.Where(mp => mp.Total > d0),
							">=" => query.Where(mp => mp.Total >= d0),
							"between" => query.Where(mp => mp.Total >= d0 && mp.Total <= d1),
							"!between" => query.Where(mp => mp.Total < d0 || mp.Total > d1),
							_ => query
						};
					}
					else if (origData == "tax")
					{
						query = condition switch
						{
							"=" => query.Where(mp => mp.Tax == d0),
							"!=" => query.Where(mp => mp.Tax != d0),
							"<" => query.Where(mp => mp.Tax < d0),
							"<=" => query.Where(mp => mp.Tax <= d0),
							">" => query.Where(mp => mp.Tax > d0),
							">=" => query.Where(mp => mp.Tax >= d0),
							"between" => query.Where(mp => mp.Tax >= d0 && mp.Tax <= d1),
							"!between" => query.Where(mp => mp.Tax < d0 || mp.Tax > d1),
							_ => query
						};
					}
					else if (origData == "discount_value")
					{
						query = condition switch
						{
							"=" => query.Where(mp => mp.DiscountValue == d0),
							"!=" => query.Where(mp => mp.DiscountValue != d0),
							"<" => query.Where(mp => mp.DiscountValue < d0),
							"<=" => query.Where(mp => mp.DiscountValue <= d0),
							">" => query.Where(mp => mp.DiscountValue > d0),
							">=" => query.Where(mp => mp.DiscountValue >= d0),
							"between" => query.Where(mp => mp.DiscountValue >= d0 && mp.DiscountValue <= d1),
							"!between" => query.Where(mp => mp.DiscountValue < d0 || mp.DiscountValue > d1),
							_ => query
						};
					}
					else if (origData == "amount")
					{
						query = condition switch
						{
							"=" => query.Where(mp => mp.Amount == d0),
							"!=" => query.Where(mp => mp.Amount != d0),
							"<" => query.Where(mp => mp.Amount < d0),
							"<=" => query.Where(mp => mp.Amount <= d0),
							">" => query.Where(mp => mp.Amount > d0),
							">=" => query.Where(mp => mp.Amount >= d0),
							"between" => query.Where(mp => mp.Amount >= d0 && mp.Amount <= d1),
							"!between" => query.Where(mp => mp.Amount < d0 || mp.Amount > d1),
							_ => query
						};
					}
				}
				else if (origData == "date")
				{
					if (DateTime.TryParse(value0, out var dt0))
					{
						var nextDay = dt0.Date.AddDays(1);
						DateTime dt1;
						
						switch (condition)
						{
							case "=":
							case "contains":
								query = query.Where(mp => mp.Date >= dt0.Date && mp.Date < nextDay);
								break;
							case "!=":
							case "!contains":
								query = query.Where(mp => mp.Date < dt0.Date || mp.Date >= nextDay);
								break;
							case "<":
								query = query.Where(mp => mp.Date < dt0.Date);
								break;
							case "<=":
								query = query.Where(mp => mp.Date < nextDay);
								break;
							case ">":
								query = query.Where(mp => mp.Date >= nextDay);
								break;
							case ">=":
								query = query.Where(mp => mp.Date >= dt0.Date);
								break;
							case "between":
								if (DateTime.TryParse(value1, out dt1))
								{
									var endDay = dt1.Date.AddDays(1);
									query = query.Where(mp => mp.Date >= dt0.Date && mp.Date < endDay);
								}
								break;
							case "!between":
								if (DateTime.TryParse(value1, out dt1))
								{
									var endDay = dt1.Date.AddDays(1);
									query = query.Where(mp => mp.Date < dt0.Date || mp.Date >= endDay);
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
						? query.OrderByDescending(mp => mp.Supplier.Name) 
						: query.OrderBy(mp => mp.Supplier.Name);
					break;
				case "2":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.Total) 
						: query.OrderBy(mp => mp.Total);
					break;
				case "3":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.Tax) 
						: query.OrderBy(mp => mp.Tax);
					break;
				case "4":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.DiscountValue) 
						: query.OrderBy(mp => mp.DiscountValue);
					break;
				case "5":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.Amount) 
						: query.OrderBy(mp => mp.Amount);
					break;
				case "6":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.Date) 
						: query.OrderBy(mp => mp.Date);
					break;
				default:
					query = query.OrderByDescending(mp => mp.CreatedDate);
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

		var canView = User.HasPermission(Permissions.Purchase.View);
		var canEdit = User.HasPermission(Permissions.Purchase.Edit);
		var canDelete = User.HasPermission(Permissions.Purchase.Delete);
		var canAction = canView || canEdit || canDelete;
		
		var formattedData = rawData.Select(mp =>
		{
			var actionHtml = new StringBuilder();
		
			if (canAction)
			{
				if (canView || canEdit)
				{
					actionHtml.Append("""
					                  <div class="dropdown d-inline-block">
					                  	<a class="text-primary edit dropdown-toggle" data-toggle="dropdown">
					                  		<i class="las la-ellipsis-h"></i>
					                  	</a>
					                  	<ul class="dropdown-menu dropdown-menu-right">
					                  """);
		
					if (canView)
					{
						actionHtml.Append($"""
						                   <li>
						                   	<a href="/Medicines/Purchase/View/{mp.Id}">
						                   		<i class="las la-laptop pr-2"></i>View
						                   	</a>
						                   </li>
						                   """);
					}
		
					if (canEdit)
					{
						actionHtml.Append($"""
						                   <li>
						                   	<a href="/Medicines/Purchase/Edit/{mp.Id}">
						                   		<i class="las la-edit pr-2"></i>Edit
						                   	</a>
						                   </li>
						                   """);
					}
		
					actionHtml.Append("""
					                  	</ul>
					                  </div>
					                  """);
				}
		
				if (canDelete)
				{
					actionHtml.Append($"""
					                   <a class="table-delete text-danger delete" data-toggle="tooltip" title="Delete">
					                   	<i class="las la-trash-alt"></i>
					                   	<input type="hidden" value="{mp.Id}">
					                   </a>
					                   """);
				}
			}
		
			return new
			{
				id = mp.Id,
				name = mp.Supplier.Name,
				total = "Rs " + mp.Total.ToString("F2"),
				tax = "Rs " + mp.Tax.ToString("F2"),
				discount_value = "Rs " + mp.DiscountValue.ToString("F2"),
				amount = "Rs " + mp.Amount.ToString("F2"),
				date = mp.Date.ToString("yyyy-MM-dd"),
				action = actionHtml.ToString()
			};
		}).ToList();

		return Json(new
		{
			draw,
			recordsTotal = totalRecords,
			recordsFiltered = filteredRecords,
			data = formattedData
		});
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Purchase.View)]
	public async Task<IActionResult> View(int id)
	{
		var purchase = await context.Purchases
			.Include(mp => mp.Supplier)
			.Include(mp => mp.Batches)
				.ThenInclude(b => b.Medicine)
			.FirstOrDefaultAsync(mp => mp.Id == id);

		if (purchase != null) return View(purchase);
				
		TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Purchase record does not exist in database!" });
		return RedirectToAction(nameof(Index));

	}

	[HttpGet]
	[Authorize(Policy = Permissions.Purchase.Add)]
	public async Task<IActionResult> Add()
	{
		var medicineViewModel = new PurchaseFormModel
		{
			Suppliers = await GetSuppliersAsync()
		};
		
		return View("PurchaseForm", medicineViewModel);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.Add)]
	public async Task<IActionResult> Add(PurchaseFormModel model)
	{
		
		model.Suppliers = await GetSuppliersAsync();
		
		if (!ModelState.IsValid)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Please check your inputs." });
			return View("PurchaseForm", model);	
		}
		
		if (model.Items.Count == 0)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Missing purchase items." });
			return View("PurchaseForm", model);
		}

		await using var transaction = await context.Database.BeginTransactionAsync();
		try
		{

			var purchase = new Purchase(
				model.Supplier,
				model.Date,
				model.Total,
				model.Tax,
				model.Discount,
				model.Amount,
				model.Note
			);

			context.Purchases.Add(purchase);
			await context.SaveChangesAsync();

			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (var item in model.Items)
			{
				var batch = new Batch(
					item.Batch,
					item.Expiry,
					item.Pqty,
					item.Qty,
					item.Saleprice,
					item.Purchaseprice,
					item.Gross,
					item.Discounttype,
					item.Discount,
					item.Discountvalue,
					item.Tax,
					item.Taxprice,
					item.Price,
					item.MedicineId,
					purchase.Id
				);
				purchase.AddBatch(batch);
			}

			await context.SaveChangesAsync();
			await transaction.CommitAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Purchase created successfully." });
			return RedirectToAction(nameof(Index));
		}	
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error saving purchase: " + ex.Message });
			return View("PurchaseForm", model);
		}
	}

	[HttpGet("{id:int}")]
		[Authorize(Policy = Permissions.Purchase.Edit)]
	public async Task<IActionResult> Edit(int id)
	{
		var purchase = await context.Purchases
			.Include(mp => mp.Supplier)
			.Include(mp => mp.Batches)
				.ThenInclude(b => b.Medicine)
			.FirstOrDefaultAsync(mp => mp.Id == id);

		if (purchase == null)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "warning", value = "Purchase record does not exist in database!" });
			return RedirectToAction(nameof(Index));
		}
		
		var model = new PurchaseFormModel
		{
			Id = purchase.Id,
			Supplier = purchase.SupplierId,
			Date = purchase.Date,
			Note = purchase.Note,
			Total = purchase.Total,
			Tax = purchase.Tax,
			Discount = purchase.DiscountValue,
			Amount = purchase.Amount,
			Suppliers = await GetSuppliersAsync(),
			Items = purchase.Batches.Select(b => new PurchaseItemFormModel
			{
				Id = b.Id,
				MedicineId = b.MedicineId,
				Name = b.Medicine.Name,
				Batch = b.BatchName,
				Expiry = b.Expiry,
				Pqty = b.Pqty,
				Qty = b.Qty,
				Purchaseprice = b.PurchasePrice,
				Saleprice = b.SalePrice,
				Gross = b.Gross,
				Discounttype = b.DiscountType,
				Discount = b.Discount,
				Discountvalue = b.DiscountValue,
				Tax = b.Tax,
				Taxprice = b.TaxPrice,
				Price = b.Price
			}).ToList()
		};

		return View("PurchaseForm", model);
	}

	[HttpPost("{id:int}")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.Edit)]
	public async Task<IActionResult> Edit(int id, PurchaseFormModel model)
	{
		if (id != model.Id) return NotFound();
		
		model.Suppliers = await GetSuppliersAsync();

		if (!ModelState.IsValid)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Please check your inputs." });
			return View("PurchaseForm", model);
		}

		if (model.Items.Count == 0)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Missing purchase items." });
			return View("PurchaseForm", model);
		}

		await using var transaction = await context.Database.BeginTransactionAsync();
		try
		{
			var existingPurchase = await context.Purchases
				.Include(mp => mp.Batches)
				
				.FirstOrDefaultAsync(mp => mp.Id == id);

			if (existingPurchase == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Purchase not found." });
				return RedirectToAction(nameof(Index));
			}

			existingPurchase.Update(
				model.Supplier,
				model.Date,
				model.Total,
				model.Tax,
				model.Discount,
				model.Amount,
				model.Note
			);

			var submittedBatchIds = model.Items.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToList();

			var batchesToRemove = existingPurchase.Batches.Where(b => !submittedBatchIds.Contains(b.Id)).ToList();
			foreach (var bToRemove in batchesToRemove)
			{
				context.Batches.Remove(bToRemove);
			}

			foreach (var item in model.Items)
			{
				if (item.Id.HasValue)
				{
					var existingBatch = existingPurchase.Batches.FirstOrDefault(b => b.Id == item.Id.Value);
					
					// ReSharper disable once UseNullPropagation
					if (existingBatch != null)
					{
						existingBatch.Update(
							item.Batch,
							item.Expiry,
							item.Pqty,
							item.Qty,
							item.Saleprice,
							item.Purchaseprice,
							item.Gross,
							item.Discounttype,
							item.Discount,
							item.Discountvalue,
							item.Tax,
							item.Taxprice,
							item.Price,
							item.MedicineId,
							existingPurchase.Id
						);
					}
					
				}
				else
				{
					var newBatch = new Batch(
						item.Batch,
						item.Expiry,
						item.Pqty,
						item.Qty,
						item.Saleprice,
						item.Purchaseprice,
						item.Gross,
						item.Discounttype,
						item.Discount,
						item.Discountvalue,
						item.Tax,
						item.Taxprice,
						item.Price,
						item.MedicineId,
						existingPurchase.Id
					);
					existingPurchase.AddBatch(newBatch);
				}
			}

			await context.SaveChangesAsync();
			await transaction.CommitAsync();
			

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Purchase updated successfully." });
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error updating purchase: " + ex.Message });
			return View("PurchaseForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.Delete)]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var purchase = await context.Purchases.FirstOrDefaultAsync(mp => mp.Id == id);
			if (purchase == null)
			{
				TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Purchase record not found." });
				return RedirectToAction(nameof(Index));
			}

			context.Purchases.Remove(purchase);
			await context.SaveChangesAsync();

			TempData["Message"] = JsonSerializer.Serialize(new { alert = "success", value = "Purchase deleted successfully." });
		}
		catch (Exception ex)
		{
			TempData["Message"] = JsonSerializer.Serialize(new { alert = "error", value = "Error deleting purchase: " + ex.Message });
		}

		return RedirectToAction(nameof(Index));
	}
	
	private async Task<IEnumerable<SelectListItem>> GetSuppliersAsync()
	{
		return await context.Suppliers.OrderBy(s => s.Name)
			.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.Name
			})
			.ToListAsync();
	}
}
