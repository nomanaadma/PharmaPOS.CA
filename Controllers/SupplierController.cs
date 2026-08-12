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
[Route("Suppliers/[action]")]
public class SupplierController(
	ApplicationDbContext context
) : BaseController
{
	[HttpGet]
	[Route("/Suppliers")]
	[Authorize(Policy = Permissions.Suppliers.List)]
	public async Task<IActionResult> Index()
	{
		var suppliersQuery = await context.Suppliers
			.OrderBy(s => s.Id)
			.ToListAsync();

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
	public async Task<IActionResult> Add(SupplierViewModel model)
	{
		if (!ModelState.IsValid)
		{
			return View("SupplierForm", model);
		}

		if (await context.Suppliers.AnyAsync(s => s.Email == model.Email))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("SupplierForm", model);
		}

		try
		{
			var supplier = new Supplier(model.Name, model.Email, model.Phone, model.Address);
			context.Suppliers.Add(supplier);
			await context.SaveChangesAsync();

			NotifySuccess("Supplier created successfully.");
			return RedirectToAction(nameof(Index));
		}
		catch (Exception ex)
		{
			NotifyError("Error saving supplier: " + ex.Message);
			return View("SupplierForm", model);
		}
	}

	[HttpGet("{id:int}")]
	[Authorize(Policy = Permissions.Suppliers.Edit)]
	public async Task<IActionResult> Edit(int id)
	{
		var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
		if (supplier == null)
		{
			NotifyError("Supplier not found.");
			return RedirectToAction(nameof(Index));
		}

		var model = SettingMapper.MapToSupplier(supplier);

		return View("SupplierForm", model);
	}
	
	[HttpPost("{id:int}")]
	[ValidateAntiForgeryToken]
	[Authorize(Policy = Permissions.Suppliers.Edit)]
	public async Task<IActionResult> Edit(int id, SupplierViewModel model)
	{
		if (id != model.Id)
		{
			NotifyError("Bad Request: ID mismatch.");
			return RedirectToAction(nameof(Index));
		}
		
		if (!model.Id.HasValue)
		{
			NotifyError("Supplier ID is required for editing.");
			return RedirectToAction(nameof(Index));
		}

		if (!ModelState.IsValid)
		{
			return View("SupplierForm", model);
		}

		if (await context.Suppliers.AnyAsync(s => s.Email == model.Email && s.Id != model.Id.Value))
		{
			ModelState.AddModelError(nameof(model.Email), "Email Address must be unique.");
			return View("SupplierForm", model);
		}

		try
		{
			var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == model.Id.Value);
			if (supplier == null)
			{
				NotifyError("Supplier not found.");
				return RedirectToAction(nameof(Index));
			}

			supplier.Update(model.Name, model.Email, model.Phone, model.Address);
			await context.SaveChangesAsync();

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
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
			if (supplier == null)
			{
				NotifyError("Supplier not found.");
				return RedirectToAction(nameof(Index));
			}

			context.Suppliers.Remove(supplier);
			await context.SaveChangesAsync();

			NotifySuccess("Supplier deleted successfully.");
		}
		catch (Exception ex)
		{
			NotifyError("Error deleting supplier: " + ex.Message);
		}

		return RedirectToAction(nameof(Index));
	}
}
