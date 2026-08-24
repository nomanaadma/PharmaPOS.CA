using ErrorOr;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.SupplierAggregate;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Services;

public class SupplierService(
	ApplicationDbContext context,
	ILogger<SupplierService> logger)
{
	public async Task<ErrorOr<int>> CreateSupplierAsync(SupplierViewModel model, CancellationToken ct = default)
	{
		if (await context.Suppliers.AnyAsync(s => s.Email == model.Email, ct))
			return SupplierErrors.EmailNotUnique;

		var details = new SupplierDetails(model.Name, model.Email, model.Phone, model.Address);

		var supplierResult = Supplier.Create(details);
		if (supplierResult.IsError)
			return supplierResult.Errors;

		var supplier = supplierResult.Value;
		context.Suppliers.Add(supplier);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} '{Name}' created", supplier.Id, supplier.Name);
		return supplier.Id;
	}

	public async Task<ErrorOr<int>> UpdateSupplierAsync(int id, SupplierViewModel model, CancellationToken ct = default)
	{
		var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		if (await context.Suppliers.AnyAsync(s => s.Email == model.Email && s.Id != id, ct))
			return SupplierErrors.EmailNotUnique;

		var details = new SupplierDetails(model.Name, model.Email, model.Phone, model.Address);

		var updateResult = supplier.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		await context.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} updated", id);
		return supplier.Id;
	}

	public async Task<ErrorOr<Deleted>> DeleteSupplierAsync(int id, CancellationToken ct = default)
	{
		var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		context.Suppliers.Remove(supplier);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<Supplier>> GetSupplierAsync(int id, CancellationToken ct = default)
	{
		var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		return supplier;
	}

	public async Task<List<Supplier>> GetAllSuppliersAsync(CancellationToken ct = default)
	{
		return await context.Suppliers
			.OrderBy(s => s.Id)
			.ToListAsync(ct);
	}

	/// <summary>
	/// Returns suppliers as SelectListItems for dropdowns (used by PurchaseController).
	/// </summary>
	public async Task<IEnumerable<SelectListItem>> GetSuppliersAsync(CancellationToken ct = default)
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