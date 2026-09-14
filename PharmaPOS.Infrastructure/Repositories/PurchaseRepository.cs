using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Helpers;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.PurchaseAggregate;
using PharmaPOS.Infrastructure.Data;
using PharmaPOS.Infrastructure.Helpers;

namespace PharmaPOS.Infrastructure.Repositories;

public class PurchaseRepository(
	ApplicationDbContext context,
	IMedicineRepository medicineRepo,
	ISupplierRepository supplierRepo,
	IBatchRepository batchRepo
	) : Repository<Purchase>(context), IPurchaseRepository
{
	public async Task<DataTableResponse<PurchaseListResponse>> GetFilteredPurchasesAsync(
		DataTableRequest request, 
		DataTableColumnMap<PurchaseListResponse> columns, 
		CancellationToken ct = default)
	{
		var query = DbSet
			.AsNoTracking()
			.Join(DbContext.Suppliers, p => p.SupplierId, s => s.Id, (p, s) => new PurchaseListResponse
			{
				Id = p.Id,
				Date = p.Date,
				SubTotal = p.SubTotal,
				Tax = p.Tax,
				Discount = p.Discount,
				Amount = p.Amount,
				Note = p.Note,
				SupplierName = s.Name
			});

		return await DataTableHelper.FilterAsync(query, request, columns, ct);
	}

	public async Task<PurchaseDetailsResponse?> GetPurchaseDetailsAsync(int id, CancellationToken ct = default)
	{
		var purchase = await DbSet.FindAsync([id], ct);
		if (purchase == null) return null;

		var supplier = await supplierRepo.GetByIdAsync(purchase.SupplierId, ct);
		if (supplier == null) return null;

		var batches = await batchRepo.GetBatchesByPurchaseIdAsync(id, ct);

		var medicineIds = batches.Select(b => b.MedicineId).Distinct().ToList();

		var medicineNames = await medicineRepo.GetMedicineNamesByIdsAsync(medicineIds, ct);

		return new PurchaseDetailsResponse(purchase, batches, medicineNames, supplier);
	}
}
