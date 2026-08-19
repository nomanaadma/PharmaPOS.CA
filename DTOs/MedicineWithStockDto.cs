using PharmaPOS.Domain.Entities;

namespace PharmaPOS_MVC.DTOs;

public record MedicineWithStockDto(Medicine Medicine, decimal LiveStock);