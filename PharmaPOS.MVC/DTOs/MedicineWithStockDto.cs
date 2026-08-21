
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.MVC.DTOs;

public record MedicineWithStockDto(Medicine Medicine, decimal LiveStock);