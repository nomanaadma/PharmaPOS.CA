using PharmaPOS.Domain.Entities;

namespace PharmaPOS.MVC.DTOs;

public record MedicineWithStockDto(Medicine Medicine, decimal LiveStock);