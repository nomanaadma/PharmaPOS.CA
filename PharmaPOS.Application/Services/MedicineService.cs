using ErrorOr;
using Mapster;
using Microsoft.Extensions.Logging;
using PharmaPOS.Application.Extensions;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Contracts.Requests;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.BatchAggregate;
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Application.Services;

public class MedicineService(
	IMedicineRepository medicineRepo,
	ICategoryRepository categoryRepo,
	IUnitOfWork unitOfWork,
	BatchService batchService,
	ILogger<MedicineService> logger) : IApplicationService
{
	/// <summary>
	/// Creates a new medicine.
	/// </summary>
	public async Task<ErrorOr<Created>> CreateMedicineAsync(MedicineRequest request, CancellationToken ct = default)
	{
		var details = request.Adapt<MedicineDetails>();

		var medicineResult = Medicine.Create(details);
		if (medicineResult.IsError)
			return medicineResult.Errors;

		var medicine = medicineResult.Value;
		await medicineRepo.AddAsync(medicine, ct);
		await unitOfWork.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} '{Name}' created", medicine.Id, medicine.Name);
		return Result.Created;
	}

	/// <summary>
	/// Updates an existing medicine.
	/// </summary>
	public async Task<ErrorOr<Updated>> UpdateMedicineAsync(int id, MedicineRequest request, CancellationToken ct = default)
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
		return Result.Updated;
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
	public async Task<ErrorOr<MedicineResponse>> GetMedicineAsync(int id, CancellationToken ct = default)
	{
		
		var medicine = await  medicineRepo.GetByIdAsync(id, ct);
		if (medicine == null)
		{
			return MedicineErrors.NotFound(id);
		}
		
		var categoryName = await categoryRepo.GetCategoryNameByIdAsync(medicine.CategoryId, ct);
		if (categoryName == null)
		{
			return MedicineErrors.CategoryNotFound(medicine.CategoryId);
		}
		
		var response = medicine.Adapt<MedicineResponse>() with
		{
			CategoryName = categoryName
		};

		return response;
	}

	/// <summary>
	/// Returns all medicines with live stock for the Index page.
	/// </summary>
	public async Task<List<MedicineResponse>> GetMedicinesWithStockAsync(CancellationToken ct = default)
	{
		var medicinesWithStock = await medicineRepo.GetMedicinesWithStockAsync(ct); 
		
		var categoryIds = medicinesWithStock.Select(msr => msr.medicine.CategoryId).Distinct().ToList();
		var categoriesNames = await categoryRepo.GetCategoryNamesByIdsAsync(categoryIds, ct);

		var medicinesResponse = new List<MedicineResponse>();

		foreach (var (medicine, liveStock) in medicinesWithStock)
		{
			var categoryName = categoriesNames.GetValueOrDefault(medicine.CategoryId, "");
			var responseResponse = medicine.Adapt<MedicineResponse>() with
			{
				CategoryName = categoryName,
				LiveStock = liveStock
			};
			
			medicinesResponse.Add(responseResponse);
			
		}

		return medicinesResponse;
		
	}
	
	
	public async Task<(
		IEnumerable<BatchResponse> liveBatches,
		IEnumerable<BatchResponse> badBatches
		)> 
		GetMedicineStockSummaryAsync(int medicineId, CancellationToken ct = default)
	{
		var today = DateTime.UtcNow.Date;
		var currentMonthStr = new DateTime(today.Year, today.Month, 1).ExpiryFormat();

		var batchesQuery = await batchService.GetActiveBatchesByMedicineAsync(medicineId, ct);
		
		var batchesResponse = batchesQuery.Adapt<List<BatchResponse>>();
		
		// Fetch medicine names for display
		var medicineIds = batchesResponse.Select(b => b.MedicineId).Distinct().ToList();
		var medicineNames = await GetMedicineNamesByIds(medicineIds, ct);
		
		batchesResponse = batchesResponse
			.Select(br => br with { Name = medicineNames.GetValueOrDefault(br.MedicineId, "") })
			.ToList();
		
		var liveBatches = batchesResponse.Where(b => 
				string.CompareOrdinal(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) >= 0);
		
		var badBatches = batchesResponse.Where(b => 
				string.CompareOrdinal(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonthStr) < 0)
			.Where(b => b.Qty > b.Sold);

		return (liveBatches, badBatches);

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
