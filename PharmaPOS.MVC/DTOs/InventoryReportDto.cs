using PharmaPOS.Domain.Entities;

namespace PharmaPOS.MVC.DTOs;

public record InventoryReportDto(
    Medicine Medicine,
    decimal TotalPurchase,
    decimal TotalSales,
    decimal LiveStock
);
