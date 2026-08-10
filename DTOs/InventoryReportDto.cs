using PharmaPOS_MVC.Domain.Entities;

namespace PharmaPOS_MVC.DTOs;

public record InventoryReportDto(
    Medicine Medicine,
    decimal TotalPurchase,
    decimal TotalSales,
    decimal LiveStock
);
