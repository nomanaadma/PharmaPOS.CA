using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.Entities;

namespace PharmaPOS.MVC.Helpers;

/// <summary>
/// Column configuration for Bill entity DataTables filtering.
/// Maps DataTables origData names to Bill entity expressions.
/// </summary>
public static class BillColumns
{
	public static DataTableColumnMap<Bill> GetColumnMap() => new()
	{
		GlobalSearchFilter = (query, search) =>
		{
			
			return query.Where(mb =>
				
				EF.Functions.ILike(mb.Name, $"%{search}%") ||
				(mb.Note != null &&
				 EF.Functions.ILike(mb.Note, $"%{search}%")) ||
				EF.Functions.ILike(mb.Subtotal.ToString(), $"%{search}%") ||
				EF.Functions.ILike(mb.Tax.ToString(), $"%{search}%") ||
				EF.Functions.ILike(mb.DiscountValue.ToString(), $"%{search}%") ||
				EF.Functions.ILike(mb.Amount.ToString(), $"%{search}%")
			);
			
		},

		Columns = new Dictionary<string, DataTableColumn<Bill>>
		{
			["name"] = new()
			{
				Type = ColumnType.String,
				StringSelector = mb => mb.Name
			},
			["subtotal"] = new()
			{
				Type = ColumnType.Decimal,
				DecimalSelector = mb => mb.Subtotal
			},
			["tax"] = new()
			{
				Type = ColumnType.Decimal,
				DecimalSelector = mb => mb.Tax
			},
			["discount_value"] = new()
			{
				Type = ColumnType.Decimal,
				DecimalSelector = mb => mb.DiscountValue
			},
			["amount"] = new()
			{
				Type = ColumnType.Decimal,
				DecimalSelector = mb => mb.Amount
			},
			["bill_date"] = new()
			{
				Type = ColumnType.DateTime,
				DateTimeSelector = mb => mb.Date
			}
		},

		SortColumns = new Dictionary<string, Expression<Func<Bill, object>>>
		{
			["1"] = mb => mb.Name,
			["2"] = mb => mb.Subtotal,
			["3"] = mb => mb.Tax,
			["4"] = mb => mb.DiscountValue,
			["5"] = mb => mb.Amount,
			["6"] = mb => mb.Date
		},

		DefaultSort = mb => mb.Id
	};
}
