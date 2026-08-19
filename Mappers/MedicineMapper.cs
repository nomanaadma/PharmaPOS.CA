using Microsoft.AspNetCore.Mvc.Rendering;
using PharmaPOS.Domain.Entities;
using PharmaPOS_MVC.DTOs;
using PharmaPOS_MVC.Models;

namespace PharmaPOS_MVC.Mappers;

public static class MedicineMapper
{
	public static MedicineViewModel MapToMedicine(Medicine medicine, 
		IEnumerable<SelectListItem>? medicineCategories = null, decimal? liveStock = null)
	{
		
		var medicineResponse = new MedicineViewModel
		{
			Id = medicine.Id,
			Name = medicine.Name,
			Company = medicine.Company,
			Generic = medicine.Generic,
			MedicineGroup = medicine.MedicineGroup,
			CategoryId = medicine.CategoryId,
			CategoryName = medicine.Category?.Name,
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
	
	public static IEnumerable<MedicineViewModel> MapToMedicinesWithStock(List<MedicineWithStockDto> medicines)
	{
		return medicines
			.Select(data => 
				MedicineMapper.MapToMedicine(data.Medicine, null, data.LiveStock))
			.ToList();
	}
	
}