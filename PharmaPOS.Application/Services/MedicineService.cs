using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Extensions;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.MedicineAggregate;
using PharmaPOS.Domain.MedicineCategoryAggregate;

namespace PharmaPOS.Application.Services;

public class MedicineService(
	IRepository<Medicine> medicineRepo,
	IRepository<MedicineCategory> categoryRepo,
	IRepository<Batch> batchRepo,
	BatchService batchService,
	ILogger<MedicineService> logger) : IApplicationService
{
	/// <summary>
	/// Creates a new medicine.
	/// </summary>
	public async Task<ErrorOr<int>> CreateMedicineAsync(MedicineRequest request, CancellationToken ct = default)
	{
		var details = request.Adapt<MedicineDetails>();

		var medicineResult = Medicine.Create(details);
		if (medicineResult.IsError)
			return medicineResult.Errors;

		var medicine = medicineResult.Value;
		await medicineRepo.AddAsync(medicine, ct);
		await medicineRepo.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} '{Name}' created", medicine.Id, medicine.Name);
		return medicine.Id;
	}

	/// <summary>
	/// Updates an existing medicine.
	/// </summary>
	public async Task<ErrorOr<int>> UpdateMedicineAsync(int id, MedicineRequest request, CancellationToken ct = default)
	{
		var medicine = await medicineRepo.FirstOrDefaultAsync(m => m.Id == id, ct);

		if (medicine == null)
			return MedicineErrors.NotFound(id);

		var details = request.Adapt<MedicineDetails>();

		var updateResult = medicine.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		await medicineRepo.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} '{Name}' updated", medicine.Id, medicine.Name);
		return medicine.Id;
	}

	/// <summary>
	/// Deletes a medicine.
	/// </summary>
	public async Task<ErrorOr<Deleted>> DeleteMedicineAsync(int id, CancellationToken ct = default)
	{
		var medicine = await medicineRepo.FirstOrDefaultAsync(m => m.Id == id, ct);

		if (medicine == null)
			return MedicineErrors.NotFound(id);

		medicineRepo.Remove(medicine);
		await medicineRepo.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} deleted", id);
		return Result.Deleted;
	}

	/// <summary>
	/// Loads a medicine with category for the View action.
	/// </summary>
	public async Task<ErrorOr<MedicineDetailsResponse>> GetMedicineAsync(int id, CancellationToken ct = default)
	{
		var medicine = await medicineRepo.FirstOrDefaultAsync(m => m.Id == id, ct);

		if (medicine == null)
			return MedicineErrors.NotFound(id);
		
		var categoryName = await categoryRepo.Query()
			.Where(c => c.Id == medicine.CategoryId)
			.Select(c => c.Name)
			.FirstOrDefaultAsync(ct);

		if (categoryName == null)
			return MedicineErrors.CategoryNotFound(medicine.CategoryId);

		return new MedicineDetailsResponse(medicine, categoryName);
	}

	/// <summary>
	/// Returns all medicines with live stock for the Index page.
	/// </summary>
	public async Task<MedicinesWithStockResponse> GetMedicinesWithStockAsync(CancellationToken ct = default)
	{
		var currentMonth = DateTime.UtcNow.ToString("yyyyMM");
		
		var medicines = await medicineRepo.Query()
			.OrderByDescending(m => m.CreatedDate)
			.Select(m => new MedicineStockResponse
			(
				m,
				batchRepo.Query()
					.Where(b => b.MedicineId == m.Id)
					.Where(b => b.Status == Status.Active && string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
					.Sum(b => (decimal?)(b.Pricing.Qty - b.Sold)) ?? 0m
			))
			.ToListAsync(ct);
		
		var medicineCategoryIds = medicines.Select(msr => msr.Medicine.CategoryId).Distinct().ToList();
		
		var categoriesNames = await categoryRepo.Query()
			.Where(c => medicineCategoryIds.Contains(c.Id))
			.ToDictionaryAsync(m => m.Id, m => m.Name, ct);

		return new MedicinesWithStockResponse(medicines, categoriesNames);
	}

	public async Task<MedicineStockSummaryResponse> GetMedicineStockSummaryAsync(int medicineId, CancellationToken ct = default)
	{
		var today = DateTime.UtcNow.Date;
		var currentMonthStr = new DateTime(today.Year, today.Month, 1).ExpiryFormat();

		var batchesQuery = await batchService.GetActiveBatchesByMedicineAsync(medicineId, ct);
		
		var liveBatches = batchesQuery.Where(b => 
				string.CompareOrdinal(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) >= 0);
		
		var badBatches = batchesQuery.Where(b => 
				string.CompareOrdinal(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) < 0)
			.Where(b => b.Pricing.Qty > b.Sold);
		
		// Fetch medicine names for display
		var medicineIds = batchesQuery.Select(b => b.MedicineId).Distinct().ToList();
		var medicineNames = await GetMedicineNamesByIds(medicineIds, ct);
            
		return new MedicineStockSummaryResponse(liveBatches, badBatches, medicineNames);
	}

	public async Task<Dictionary<int, string>> GetMedicineNamesByIds(List<int> medicineIds, CancellationToken ct)
	{
		return await medicineRepo.Query()
			.Where(m => medicineIds.Contains(m.Id))
			.ToDictionaryAsync(m => m.Id, m => m.Name, ct);
	}

	public async Task<object> SearchMedicinesAsync(string term, CancellationToken ct = default)
	{
		var termLower = term.ToLower();
		return await medicineRepo.Query()
			.Where(m => m.Name.ToLower().Contains(termLower))
			.Take(10)
			.Select(m => new
			{
				id = m.Id,
				label = m.Name,
				m.Generic
			})
			.ToListAsync(ct);
	}
}
