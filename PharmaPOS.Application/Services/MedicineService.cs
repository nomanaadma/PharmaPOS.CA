using ErrorOr;
using Mapster;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Extensions;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Application.Services;

public class MedicineService(
	IMedicineRepository medicineRepo,
	IUnitOfWork unitOfWork,
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
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} '{Name}' created", medicine.Id, medicine.Name);
		return medicine.Id;
	}

	/// <summary>
	/// Updates an existing medicine.
	/// </summary>
	public async Task<ErrorOr<int>> UpdateMedicineAsync(int id, MedicineRequest request, CancellationToken ct = default)
	{
		var medicine = await medicineRepo.GetByIdAsync(id, ct);

		if (medicine == null)
			return MedicineErrors.NotFound(id);

		var details = request.Adapt<MedicineDetails>();

		var updateResult = medicine.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		medicineRepo.Update(medicine);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} '{Name}' updated", medicine.Id, medicine.Name);
		return medicine.Id;
	}

	/// <summary>
	/// Deletes a medicine.
	/// </summary>
	public async Task<ErrorOr<Deleted>> DeleteMedicineAsync(int id, CancellationToken ct = default)
	{
		var medicine = await medicineRepo.GetByIdAsync(id, ct);

		if (medicine == null)
			return MedicineErrors.NotFound(id);

		medicineRepo.Remove(medicine);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} deleted", id);
		return Result.Deleted;
	}

	/// <summary>
	/// Loads a medicine with category for the View action.
	/// </summary>
	public async Task<ErrorOr<MedicineDetailsResponse>> GetMedicineAsync(int id, CancellationToken ct = default)
	{
		var response = await medicineRepo.GetMedicineDetailsAsync(id, ct);

		if (response != null) return response;
		
		var medicine = await medicineRepo.GetByIdAsync(id, ct);
		
		return medicine == null ? MedicineErrors.NotFound(id) : MedicineErrors.CategoryNotFound(medicine.CategoryId);
	}

	/// <summary>
	/// Returns all medicines with live stock for the Index page.
	/// </summary>
	public async Task<MedicinesWithStockResponse> GetMedicinesWithStockAsync(CancellationToken ct = default)
	{
		return await medicineRepo.GetMedicinesWithStockAsync(ct);
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
		return await medicineRepo.GetMedicineNamesByIdsAsync(medicineIds, ct);
	}

	public async Task<object> SearchMedicinesAsync(string term, CancellationToken ct = default)
	{
		return await medicineRepo.SearchMedicinesAsync(term, 10, ct);
	}
}
