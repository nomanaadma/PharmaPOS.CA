using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.MVC.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS.Domain.PurchaseAggregate;
using PharmaPOS.MVC.Authorization;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Mappers;
using PharmaPOS.MVC.Models;
using PharmaPOS.MVC.Helpers;

namespace PharmaPOS.MVC.Controllers;

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
		var request = DataTableRequest.FromForm(Request.Form);

		var query = context.Purchases
			.Include(mp => mp.Supplier)
			.AsQueryable();

		var result = await DataTableHelper.FilterAsync(query, request, PurchaseColumns.GetColumnMap(), ct);

		var canView = User.HasPermission(Permissions.Purchase.View);
		var canEdit = User.HasPermission(Permissions.Purchase.Edit);
		var canDelete = User.HasPermission(Permissions.Purchase.Delete);

		var formattedData = PurchaseMapper.MapToPurchases(result.Data, canView, canEdit, canDelete);

		return Json(new
		{
			draw = request.Draw,
			recordsTotal = result.TotalRecords,
			recordsFiltered = result.FilteredRecords,
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

			var details = new PurchaseDetails(
				model.Supplier,
				model.Date,
				model.SubTotal,
				model.Tax,
				model.Discount,
				model.Amount,
				model.Note
			);
			var purchase = new Purchase(details);

			context.Purchases.Add(purchase);
			await context.SaveChangesAsync(ct);

			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (var item in model.Items)
			{
				
				var pricing = PricingHelper.PricingDetails(item);

				var batchDetails = new BatchDetails(
					item.Batch,
					item.Expiry,
					item.Pqty,
					pricing,
					item.Purchaseprice,
					item.MedicineId,
					purchase.Id
				);
				var batch = new Batch(batchDetails);
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

			var details = new PurchaseDetails(
				model.Supplier,
				model.Date,
				model.SubTotal,
				model.Tax,
				model.Discount,
				model.Amount,
				model.Note
			);
			existingPurchase.Update(details);

			var submittedBatchIds = model.Items.Where(i => i.Id != 0).Select(i => i.Id).ToList();

			var batchesToRemove = existingPurchase.Batches.Where(b => !submittedBatchIds.Contains(b.Id)).ToList();
			foreach (var bToRemove in batchesToRemove)
			{
				if (bToRemove.Sold > 0)
				{
					await transaction.RollbackAsync(ct);
					NotifyError($"Cannot delete batch '{bToRemove.BatchName}' because it has already been sold.");
					return View("PurchaseForm", model);
				}
				context.Batches.Remove(bToRemove);
			}

			foreach (var item in model.Items)
			{
				
				var pricing = PricingHelper.PricingDetails(item);
				
				if (item.Id != 0)
				{
					var existingBatch = existingPurchase.Batches.FirstOrDefault(b => b.Id == item.Id);
					
					if (existingBatch == null) continue;
					
					if (pricing.Qty < existingBatch.Sold)
					{
						await transaction.RollbackAsync(ct);
						NotifyError($"Cannot update batch '{existingBatch.BatchName}'. New quantity ({pricing.Qty}) is less than the amount already sold ({existingBatch.Sold}).");
						return View("PurchaseForm", model);
					}

					var batchDetails = new BatchDetails(
						item.Batch,
						item.Expiry,
						item.Pqty,
						pricing,
						item.Purchaseprice,
						item.MedicineId,
						existingPurchase.Id
					);
					existingBatch.Update(batchDetails);

				}
				else
				{

					var batchDetails = new BatchDetails(
						item.Batch,
						item.Expiry,
						item.Pqty,
						pricing,
						item.Purchaseprice,
						item.MedicineId,
						existingPurchase.Id
					);
					var newBatch = new Batch(batchDetails);
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
			var purchase = await context.Purchases
				.Include(mp => mp.Batches)
				.FirstOrDefaultAsync(mp => mp.Id == id, ct);
			
			if (purchase == null)
			{
				NotifyError("Purchase record not found.");
				return NotFound();
			}

			if (purchase.Batches.Any(b => b.Sold > 0))
			{
				NotifyError("Cannot delete purchase because one or more batches have already been sold.");
				return RedirectToAction(nameof(Index));
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
