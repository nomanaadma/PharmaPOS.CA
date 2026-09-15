using ErrorOr;
using Mapster;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.SupplierAggregate;

namespace PharmaPOS.Application.Services;

public class SupplierService(
	ISupplierRepository supplierRepo,
	IUnitOfWork unitOfWork,
	ILogger<SupplierService> logger) : IApplicationService
{
	public async Task<ErrorOr<Created>> CreateSupplierAsync(SupplierRequest request, CancellationToken ct = default)
	{
		if (await supplierRepo.ExistsByEmailAsync(request.Email, null, ct))
			return SupplierErrors.EmailNotUnique;

		var details = request.Adapt<SupplierDetails>();

		var supplierResult = Supplier.Create(details);
		if (supplierResult.IsError)
			return supplierResult.Errors;

		var supplier = supplierResult.Value;
		await supplierRepo.AddAsync(supplier, ct);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} '{Name}' created", supplier.Id, supplier.Name);
		return Result.Created;
	}

	public async Task<ErrorOr<Updated>> UpdateSupplierAsync(int id, SupplierRequest request, CancellationToken ct = default)
	{
		var supplier = await supplierRepo.GetByIdAsync(id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		if (await supplierRepo.ExistsByEmailAsync(request.Email, id, ct))
			return SupplierErrors.EmailNotUnique;

		var details = request.Adapt<SupplierDetails>();

		var updateResult = supplier.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		supplierRepo.Update(supplier);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} updated", id);
		return Result.Updated;
	}

	public async Task<ErrorOr<Deleted>> DeleteSupplierAsync(int id, CancellationToken ct = default)
	{
		var supplier = await supplierRepo.GetByIdAsync(id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		supplierRepo.Remove(supplier);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Supplier {SupplierId} deleted", id);
		return Result.Deleted;
	}

	public async Task<ErrorOr<Supplier>> GetSupplierAsync(int id, CancellationToken ct = default)
	{
		var supplier = await supplierRepo.GetByIdAsync(id, ct);
		if (supplier == null)
			return SupplierErrors.NotFound(id);

		return supplier;
	}

	public async Task<List<Supplier>> GetAllSuppliersAsync(CancellationToken ct = default)
	{
		return await supplierRepo.GetAllAsync(ct);
	}

	/// <summary>
	/// Returns suppliers as LookupItems for dropdowns.
	/// </summary>
	public async Task<IEnumerable<LookupItem>> GetSuppliersAsync(CancellationToken ct = default)
	{
		return await supplierRepo.GetLookupListAsync(ct);
	}
}