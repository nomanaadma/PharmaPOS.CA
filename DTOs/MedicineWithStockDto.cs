using PharmaPOS_MVC.Domain.Entities;

namespace PharmaPOS_MVC.DTOs;

public record MedicineWithStockDto(Medicine Medicine, decimal LiveStock);