using Microsoft.EntityFrameworkCore;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.MedicineAggregate;
using PharmaPOS.Infrastructure.Data;

namespace PharmaPOS.Infrastructure.Repositories;

public class MedicineRepository(
	ICategoryRepository categoryRepo,
	ApplicationDbContext context) : Repository<Medicine>(context), IMedicineRepository
{
	public async Task<MedicineDetailsResponse?> GetMedicineDetailsAsync(int id, CancellationToken ct = default)
	{
		var medicine = await DbSet.FindAsync([id], ct);
		if (medicine == null) return null;

		var categoryName = await categoryRepo.GetCategoryNameByIdAsync(medicine.CategoryId, ct);
			
		if (categoryName == null) return null;

		return new MedicineDetailsResponse(medicine, categoryName);
	}

	public async Task<MedicinesWithStockResponse> GetMedicinesWithStockAsync(CancellationToken ct = default)
	{
		var currentMonth = DateTime.UtcNow.ToString("yyyyMM");

		var medicines = await DbSet
			.OrderByDescending(m => m.CreatedDate)
			.Select(m => new MedicineStockResponse
			(
				m,
				DbContext.Batches
					.Where(b => b.MedicineId == m.Id)
					.Where(b => b.Status == Status.Active && string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
					.Sum(b => (decimal?)(b.Pricing.Qty - b.Sold)) ?? 0m
			))
			.ToListAsync(ct);

		var medicineCategoryIds = medicines.Select(msr => msr.Medicine.CategoryId).Distinct().ToList();
		
		var categoriesNames = await categoryRepo.GetCategoryNamesByIdsAsync(medicineCategoryIds, ct);

		return new MedicinesWithStockResponse(medicines, categoriesNames);
	}

	public async Task<Dictionary<int, string>> GetMedicineNamesByIdsAsync(List<int> medicineIds, CancellationToken ct = default)
	{
		return await DbSet
			.Where(m => medicineIds.Contains(m.Id))
			.ToDictionaryAsync(m => m.Id, m => m.Name, ct);
	}

	public async Task<List<MedicineSearchResult>> SearchMedicinesAsync(string term, int take = 10, CancellationToken ct = default)
	{
		return await DbSet
			.Where(m => EF.Functions.ILike(m.Name, $"%{term}%"))
			.Take(take)
			.Select(m => new MedicineSearchResult
			(
				m.Id,
				m.Name,
				m.Generic
			))
			.ToListAsync(ct);
	}
}
