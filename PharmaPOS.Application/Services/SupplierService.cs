using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Application.Services;

public class SupplierService(
	IRepository<Supplier> supplierRepo,
	ILogger<SupplierService> logger) : IApplicationService
{
	public async Task<ErrorOr<int>> CreateSupplierAsync(SupplierRequest request, CancellationToken ct = default)
	{
		if (await supplierRepo.AnyAsync(s => s.Email == request.Email, ct))
			return SupplierErrors.EmailNotUnique;

		var details = request.Adapt<SupplierDetails>();

		var supplierResult = Supplier.Create(details);
		if (supplierResult.IsError)
			return supplierResult.Errors;

		var supplier = supplierResult.Value;
		await supplierRepo.AddAsync(supplier, ct);
		await supplierRepo.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} '{Name}' created", supplier.Id, supplier.Name);
		return supplier.Id;
	}

	public async Task<ErrorOr<int>> UpdateSupplierAsync(int id, SupplierRequest request, CancellationToken ct = default)
	{
		var supplier = await supplierRepo.FirstOrDefaultAsync(s => s.Id == id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		if (await supplierRepo.AnyAsync(s => s.Email == request.Email && s.Id != id, ct))
			return SupplierErrors.EmailNotUnique;

		var details = request.Adapt<SupplierDetails>();

		var updateResult = supplier.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		await supplierRepo.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} updated", id);
		return supplier.Id;
	}

	public async Task<ErrorOr<Deleted>> DeleteSupplierAsync(int id, CancellationToken ct = default)
	{
		var supplier = await supplierRepo.FirstOrDefaultAsync(s => s.Id == id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		supplierRepo.Remove(supplier);
		await supplierRepo.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<Supplier>> GetSupplierAsync(int id, CancellationToken ct = default)
	{
		var supplier = await supplierRepo.FirstOrDefaultAsync(s => s.Id == id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		return supplier;
	}

	public async Task<List<Supplier>> GetAllSuppliersAsync(CancellationToken ct = default)
	{
		return await supplierRepo.Query()
			.OrderBy(s => s.Id)
			.ToListAsync(ct);
	}

	/// <summary>
	/// Returns suppliers as LookupItems for dropdowns.
	/// </summary>
	public async Task<IEnumerable<LookupItem>> GetSuppliersAsync(CancellationToken ct = default)
	{
		return await supplierRepo.Query()
			.OrderBy(s => s.Name)
			.Select(s => new LookupItem(s.Id.ToString(), s.Name))
			.ToListAsync(ct);
	}
}