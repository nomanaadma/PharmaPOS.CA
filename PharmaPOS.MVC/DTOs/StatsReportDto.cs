namespace PharmaPOS.MVC.DTOs;

public record StatsReportDto
(
	DateTime StartDate,
	DateTime EndDate,
	int? MedicineId,
	string? MedicineName,
	decimal SalesAmount,
	decimal PurchaseAmount,
	decimal? Quantity,
	decimal TaxAmount,
	decimal DiscountAmount
);