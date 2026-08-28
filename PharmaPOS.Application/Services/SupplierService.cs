using ErrorOr;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Data;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Application.Services;

public class SupplierService(
	ApplicationDbContext context,
	ILogger<SupplierService> logger) : IApplicationService
{
	public async Task<ErrorOr<int>> CreateSupplierAsync(SupplierRequest request, CancellationToken ct = default)
	{
		if (await context.Suppliers.AnyAsync(s => s.Email == request.Email, ct))
			return SupplierErrors.EmailNotUnique;

		var details = request.Adapt<SupplierDetails>();

		var supplierResult = Supplier.Create(details);
		if (supplierResult.IsError)
			return supplierResult.Errors;

		var supplier = supplierResult.Value;
		context.Suppliers.Add(supplier);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} '{Name}' created", supplier.Id, supplier.Name);
		return supplier.Id;
	}

	public async Task<ErrorOr<int>> UpdateSupplierAsync(int id, SupplierRequest request, CancellationToken ct = default)
	{
		var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		if (await context.Suppliers.AnyAsync(s => s.Email == request.Email && s.Id != id, ct))
			return SupplierErrors.EmailNotUnique;

		var details = request.Adapt<SupplierDetails>();

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