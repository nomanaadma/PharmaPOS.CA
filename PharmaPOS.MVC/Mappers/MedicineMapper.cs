using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.MedicineAggregate;
using PharmaPOS.Domain.MedicineCategoryAggregate;
using PharmaPOS.MVC.Models;

namespace PharmaPOS.MVC.Mappers;

public static class MedicineMapper
{
	public static MedicineViewModel MapToMedicine(MedicineDetailsResponse response, 
		IEnumerable<SelectListItem>? medicineCategories = null, decimal? liveStock = null)
	{

		var (medicine, category) = response;
		
		var medicineResponse = new MedicineViewModel
		{
			Id = medicine.Id,
			Name = medicine.Name,
			Company = medicine.Company,
			Generic = medicine.Generic,
			MedicineGroup = medicine.MedicineGroup,
			CategoryId = medicine.CategoryId,
			CategoryName = category,
			StoreBox = medicine.StoreBox,
			MinLevel = medicine.MinLevel,
			ReOrderLevel = medicine.ReOrderLevel,
			Unit = medicine.Unit,
			UnitPacking = medicine.UnitPacking,
			Note = medicine.Note,
			CreatedDate = medicine.CreatedDate
		};

		if (medicineCategories != null)
		{
			medicineResponse.Categories = medicineCategories;
		}

		if (liveStock != null)
		{
			medicineResponse.LiveStock = liveStock.Value;
		} 

		return medicineResponse;
	}
	
	public static IEnumerable<MedicineViewModel> MapToMedicinesWithStock(MedicinesWithStockResponse response)
	{

		var (medicines, categories) = response;
		
		return medicines
			.Select(data =>
			{
				var categoryName = categories.GetValueOrDefault(data.Medicine.CategoryId, "");
				var detailsResponse = new MedicineDetailsResponse(data.Medicine, categoryName);
				return MedicineMapper.MapToMedicine(detailsResponse, null, data.LiveStock);
			})
			.ToList();
	}
	
}