using PharmaPOS.Domain.BillingAggregate;

namespace PharmaPOS.MVC.Extensions;

public static class QueryExtension
{
	public static IQueryable<BillItem> IdOrNullWhere(this IQueryable<BillItem> query, int id)
	{
		return id == 0 ? query.Where(bi => true) : query.Where(bi => bi.MedicineId == id);
	}
}