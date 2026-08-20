using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PharmaPOS.Domain.Entities;

namespace PharmaPOS.MVC.Helpers;

/// <summary>
/// Column configuration for Purchase entity DataTables filtering.
/// Maps DataTables origData names to Purchase/Supplier entity expressions.
/// </summary>
public static class PurchaseColumns
{
	public static DataTableColumnMap<Purchase> GetColumnMap() => new()
	{
		GlobalSearchFilter = (query, search) =>
		{
			
			return query.Where(mp =>
				EF.Functions.ILike(mp.Supplier.Name, $"%{search}%") ||
    
				(mp.Note != null &&
				 EF.Functions.ILike(mp.Note, $"%{search}%")) ||

				EF.Functions.ILike(mp.Subtotal.ToString(), $"%{search}%") ||
				EF.Functions.ILike(mp.Tax.ToString(), $"%{search}%") ||
				EF.Functions.ILike(mp.DiscountValue.ToString(), $"%{search}%") ||
				EF.Functions.ILike(mp.Amount.ToString(), $"%{search}%")
			);
			
		},

		Columns = new Dictionary<string, DataTableColumn<Purchase>>
		{
			["name"] = new()
			{
				Type = ColumnType.String,
				StringSelector = mp => mp.Supplier.Name
			},
			["total"] = new()
			{
				Type = ColumnType.Decimal,
				DecimalSelector = mp => mp.Subtotal
			},
			["tax"] = new()
			{
				Type = ColumnType.Decimal,
				DecimalSelector = mp => mp.Tax
			},
			["discount_value"] = new()
			{
				Type = ColumnType.Decimal,
				DecimalSelector = mp => mp.DiscountValue
			},
			["amount"] = new()
			{
				Type = ColumnType.Decimal,
				DecimalSelector = mp => mp.Amount
			},
			["date"] = new()
			{
				Type = ColumnType.DateTime,
				DateTimeSelector = mp => mp.Date
			}
		},

		SortColumns = new Dictionary<string, Expression<Func<Purchase, object>>>
		{
			["1"] = mp => mp.Supplier.Name,
			["2"] = mp => mp.Subtotal,
			["3"] = mp => mp.Tax,
			["4"] = mp => mp.DiscountValue,
			["5"] = mp => mp.Amount,
			["6"] = mp => mp.Date
		},

		DefaultSort = mp => mp.Id
	};
}
