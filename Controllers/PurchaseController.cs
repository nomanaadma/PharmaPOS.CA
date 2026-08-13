using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Domain.Entities;
using PharmaPOS_MVC.Domain.Enums;
using PharmaPOS_MVC.Extensions;
using PharmaPOS_MVC.Mappers;
using PharmaPOS_MVC.Models;
using PharmaPOS_MVC.Domain.ValueObjects;
using PharmaPOS_MVC.Helpers;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Medicines/Purchase/[action]")]
public class PurchaseController(
	ApplicationDbContext context
) : BaseController
{
	[HttpGet("/Medicines/Purchase")]
	[Authorize(Policy = Permissions.Purchase.List)]
	public IActionResult Index()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.List)]
	public async Task<IActionResult> Filter(CancellationToken ct = default)
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
		var totalRecords = await query.CountAsync(ct);

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
		var filteredRecords = await query.CountAsync(ct);

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

		var rawData = await pagedQuery.ToListAsync(ct);

		var canView = User.HasPermission(Permissions.Purchase.View);
		var canEdit = User.HasPermission(Permissions.Purchase.Edit);
		var canDelete = User.HasPermission(Permissions.Purchase.Delete);
		var canAction = canView || canEdit || canDelete;
		
		var formattedData = PurchaseMapper.MapToPurchases(rawData, canView, canEdit, canDelete);

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
	public async Task<IActionResult> View(int id, CancellationToken ct = default)
	{
		var purchase = await context.Purchases
			.Include(mp => mp.Supplier)
			.Include(mp => mp.Batches)
				.ThenInclude(b => b.Medicine)
			.FirstOrDefaultAsync(mp => mp.Id == id, ct);


		if (purchase == null)
		{
			NotifyWarning("Purchase record does not exist in database!");
			return NotFound();
		}

		var model = PurchaseMapper.MapToPurchaseView(purchase);

		return View(model);

	}

	[HttpGet]
	[Authorize(Policy = Permissions.Purchase.Add)]
	public async Task<IActionResult> Add(CancellationToken ct = default)
	{
		var medicineViewModel = new PurchaseFormModel
		{
			Suppliers = await GetSuppliersAsync(ct)
		};
		
		return View("PurchaseForm", medicineViewModel);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.Add)]
	public async Task<IActionResult> Add(PurchaseFormModel model, CancellationToken ct = default)
	{
		
		model.Suppliers = await GetSuppliersAsync(ct);
		
		if (!ModelState.IsValid)
		{
			NotifyError("Please check your inputs.");
			return View("PurchaseForm", model);	
		}
		
		if (model.Items.Count == 0)
		{
			NotifyError("Missing purchase items.");
			return View("PurchaseForm", model);
		}

		await using var transaction = await context.Database.BeginTransactionAsync(ct);
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
			await context.SaveChangesAsync(ct);

			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (var item in model.Items)
			{
				
				var pricing = PricingHelper.PricingDetails(item);

				var batch = new Batch(
					item.Batch,
					item.Expiry,
					item.Pqty,
					pricing,
					item.Purchaseprice,
					item.MedicineId,
					purchase.Id
				);
				purchase.AddBatch(batch);
			}

			await context.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);

			NotifySuccess("Purchase created successfully.");
			return RedirectToAction(nameof(Index));
		}	
		catch (Exception ex)
		{
			await transaction.RollbackAsync(ct);
			NotifyError("Error saving purchase: " + ex.Message);
			return View("PurchaseForm", model);
		}
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Purchase.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var purchase = await context.Purchases
			.Include(mp => mp.Supplier)
			.Include(mp => mp.Batches)
				.ThenInclude(b => b.Medicine)
			.FirstOrDefaultAsync(mp => mp.Id == id, ct);

		if (purchase == null)
		{
			NotifyWarning("Purchase record does not exist in database!");
			return NotFound();
		}
		
		var suppliers = await GetSuppliersAsync(ct);
		var model = PurchaseMapper.MapToPurchaseForm(purchase, suppliers);

		return View("PurchaseForm", model);
	}

	[HttpPost("{id:int}")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.Edit)]
	public async Task<IActionResult> Edit(int id, PurchaseFormModel model, CancellationToken ct = default)
	{
		
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return BadRequest();
		}
		
		
		model.Suppliers = await GetSuppliersAsync(ct);

		if (!ModelState.IsValid)
		{
			NotifyError("Please check your inputs.");
			return View("PurchaseForm", model);
		}

		if (model.Items.Count == 0)
		{
			NotifyError("Missing purchase items.");
			return View("PurchaseForm", model);
		}

		await using var transaction = await context.Database.BeginTransactionAsync(ct);
		try
		{
			var existingPurchase = await context.Purchases
				.Include(mp => mp.Batches)
				
				.FirstOrDefaultAsync(mp => mp.Id == id, ct);

			if (existingPurchase == null)
			{
				NotifyError("Purchase not found.");
				return NotFound();
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
				
				var pricing = PricingHelper.PricingDetails(item);
				
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
							pricing,
							item.Purchaseprice,
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
						pricing,
						item.Purchaseprice,
						item.MedicineId,
						existingPurchase.Id
					);
					existingPurchase.AddBatch(newBatch);
				}
			}

			await context.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);
			

			NotifySuccess("Purchase updated successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(ct);
			NotifyError("Error updating purchase: " + ex.Message);
			return View("PurchaseForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Purchase.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		try
		{
			var purchase = await context.Purchases.FirstOrDefaultAsync(mp => mp.Id == id, ct);
			if (purchase == null)
			{
				NotifyError("Purchase record not found.");
				return NotFound();
			}

			context.Purchases.Remove(purchase);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Purchase deleted successfully.");
		}
		catch (Exception ex)
		{
			NotifyError("Error deleting purchase: " + ex.Message);
		}

		return RedirectToAction(nameof(Index));
	}
	
	private async Task<IEnumerable<SelectListItem>> GetSuppliersAsync(CancellationToken ct = default)
	{
		return await context.Suppliers.OrderBy(s => s.Name)
			.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.Name
			})
			.ToListAsync(ct);
	}
}
