using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS.Domain.Entities;
using PharmaPOS_MVC.Models;
using PharmaPOS_MVC.Authorization;
using PharmaPOS_MVC.Mappers;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
public class SuppliersController(
	ApplicationDbContext context
) : BaseController
{
	[HttpGet]
	[Authorize(Policy = Permissions.Suppliers.List)]
	public async Task<IActionResult> Index(CancellationToken ct = default)
	{
		var suppliersQuery = await context.Suppliers
			.OrderBy(s => s.Id)
			.ToListAsync(ct);

		var suppliers = SettingMapper.MapToSuppliers(suppliersQuery);
		return View(suppliers);
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Suppliers.Add)]
	public IActionResult Add()
	{
		return View("SupplierForm", new SupplierViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Suppliers.Add)]
	public async Task<IActionResult> Add(SupplierViewModel model, CancellationToken ct = default)
	{
		if (!ModelState.IsValid)
		{
			return View("SupplierForm", model);
		}

		if (await context.Suppliers.AnyAsync(s => s.Email == model.Email, ct))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("SupplierForm", model);
		}

		try
		{
			var supplier = new Supplier(model.Name, model.Email, model.Phone, model.Address);
			context.Suppliers.Add(supplier);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Supplier created successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error saving supplier: " + ex.Message);
			return View("SupplierForm", model);
		}
	}

	[HttpGet]
	[Authorize(Policy = Permissions.Suppliers.Edit)]
	public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
	{
		var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);
		if (supplier == null)
		{
			NotifyError("Supplier not found.");
			return NotFound();
		}

		var model = SettingMapper.MapToSupplier(supplier);

		return View("SupplierForm", model);
	}
	
	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Suppliers.Edit)]
	public async Task<IActionResult> Edit(int id, SupplierViewModel model, CancellationToken ct = default)
	{
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return BadRequest();
		}
		
		if (!model.Id.HasValue)
		{
			NotifyError("Supplier ID is required for editing.");
			return BadRequest();
		}

		if (!ModelState.IsValid)
		{
			return View("SupplierForm", model);
		}

		if (await context.Suppliers.AnyAsync(s => s.Email == model.Email && s.Id != model.Id.Value, ct))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("SupplierForm", model);
		}

		try
		{
			var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == model.Id.Value, ct);
			if (supplier == null)
			{
				NotifyError("Supplier not found.");
				return NotFound();
			}

			supplier.Update(model.Name, model.Email, model.Phone, model.Address);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Supplier updated successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error updating supplier: " + ex.Message);
			return View("SupplierForm", model);
		}
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Suppliers.Delete)]
	public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
	{
		try
		{
			var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);
			if (supplier == null)
			{
				NotifyError("Supplier not found.");
				return NotFound();
			}

			context.Suppliers.Remove(supplier);
			await context.SaveChangesAsync(ct);

			NotifySuccess("Supplier deleted successfully.");
		}
		catch (Exception ex)
		{
			NotifyError("Error deleting supplier: " + ex.Message);
		}

		return RedirectToAction(nameof(Index));
	}
}
