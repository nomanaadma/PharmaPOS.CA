using ErrorOr;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.Common.Enums;
using PharmaPOS.Domain.MedicineAggregate;
using PharmaPOS.MVC.Data;
using PharmaPOS.MVC.DTOs;
using PharmaPOS.MVC.Extensions;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Services;

public class MedicineService(
	ApplicationDbContext context,
	ILogger<MedicineService> logger)
{
	/// <summary>
	/// Creates a new medicine.
	/// </summary>
	public async Task<ErrorOr<int>> CreateMedicineAsync(MedicineViewModel model, CancellationToken ct = default)
	{
		var details = new MedicineDetails(
			model.Name,
			model.Company,
			model.Generic,
			model.MedicineGroup,
			model.CategoryId,
			model.StoreBox,
			model.MinLevel,
			model.ReOrderLevel,
			model.Unit,
			model.UnitPacking,
			model.Note
		);

		var medicineResult = Medicine.Create(details);
		if (medicineResult.IsError)
			return medicineResult.Errors;

		var medicine = medicineResult.Value;
		context.Medicines.Add(medicine);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} '{Name}' created", medicine.Id, medicine.Name);
		return medicine.Id;
	}

	/// <summary>
	/// Updates an existing medicine.
	/// </summary>
	public async Task<ErrorOr<int>> UpdateMedicineAsync(int id, MedicineViewModel model, CancellationToken ct = default)
	{
		var medicine = await context.Medicines.FirstOrDefaultAsync(m => m.Id == id, ct);

		if (medicine == null)
			return MedicineErrors.NotFound(id);

		var details = new MedicineDetails(
			model.Name,
			model.Company,
			model.Generic,
			model.MedicineGroup,
			model.CategoryId,
			model.StoreBox,
			model.MinLevel,
			model.ReOrderLevel,
			model.Unit,
			model.UnitPacking,
			model.Note
		);

		var updateResult = medicine.Update(details);
		if (updateResult.IsError)
			return updateResult.Errors;

		await context.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} '{Name}' updated", medicine.Id, medicine.Name);
		return medicine.Id;
	}

	/// <summary>
	/// Deletes a medicine.
	/// </summary>
	public async Task<ErrorOr<Deleted>> DeleteMedicineAsync(int id, CancellationToken ct = default)
	{
		var medicine = await context.Medicines.FirstOrDefaultAsync(m => m.Id == id, ct);

		if (medicine == null)
			return MedicineErrors.NotFound(id);

		context.Medicines.Remove(medicine);
		await context.SaveChangesAsync(ct);

		logger.LogInformation("Medicine {MedicineId} deleted", id);
		return Result.Deleted;
	}

	/// <summary>
	/// Loads a medicine with category for the View action.
	/// </summary>
	public async Task<ErrorOr<Medicine>> GetMedicineAsync(int id, CancellationToken ct = default)
	{
		var medicine = await context.Medicines
			.Include(m => m.Category)
			.FirstOrDefaultAsync(m => m.Id == id, ct);

		if (medicine == null)
			return MedicineErrors.NotFound(id);

		return medicine;
	}

	/// <summary>
	/// Returns all medicines with live stock for the Index page.
	/// </summary>
	public async Task<List<MedicineWithStockDto>> GetMedicinesWithStockAsync(CancellationToken ct = default)
	{
		var currentMonth = DateTime.UtcNow.ExpiryFormat();

		return await context.Medicines
			.Include(m => m.Category)
			.OrderByDescending(m => m.CreatedDate)
			.Select(m => new MedicineWithStockDto
			(
				Medicine: m,
				LiveStock: m.Batches
					.Where(b => b.Status == Status.Active && string.Compare(b.Expiry.Substring(3, 4) + b.Expiry.Substring(0, 2), currentMonth) >= 0)
					.Sum(b => (decimal?)(b.Pricing.Qty - b.Sold)) ?? 0m
			))
			.ToListAsync(ct);
	}
	
}
