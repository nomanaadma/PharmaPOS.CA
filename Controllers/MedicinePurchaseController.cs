using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;

namespace PharmaPOS_MVC.Controllers;

[Authorize]
[Route("Medicines/Purchase/[action]")]
public class MedicinePurchaseController(
	ApplicationDbContext context
) : Controller
{
	[HttpGet]
	[Route("/Medicines/Purchase")]
	public IActionResult Index()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Filter()
	{
		var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault() ?? "1");
		var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
		var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
		var searchValue = Request.Form["search[value]"].FirstOrDefault();
		var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
		var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "asc";

		var query = context.MedicinePurchases
			.Include(mp => mp.Supplier)
			.AsQueryable();

		// Total count
		var totalRecords = await query.CountAsync();

		// Apply search
		if (!string.IsNullOrWhiteSpace(searchValue))
		{
			decimal.TryParse(searchValue, out var searchDecimal);
			query = query.Where(mp => 
				EF.Functions.ILike(mp.Supplier.Name, $"%{searchValue}%") ||
				(mp.Note != null && EF.Functions.ILike(mp.Note, $"%{searchValue}%")) ||
				(searchDecimal != 0 && (mp.Total == searchDecimal || mp.Tax == searchDecimal || mp.DiscountValue == searchDecimal || mp.Amount == searchDecimal))
			);
		}

		// Apply SearchBuilder criteria
		int sbIndex = 0;
		while (Request.Form.ContainsKey($"searchBuilder[criteria][{sbIndex}][condition]"))
		{
			var origData = Request.Form[$"searchBuilder[criteria][{sbIndex}][origData]"].FirstOrDefault();
			var condition = Request.Form[$"searchBuilder[criteria][{sbIndex}][condition]"].FirstOrDefault();
			var type = Request.Form[$"searchBuilder[criteria][{sbIndex}][type]"].FirstOrDefault();
			var value0 = Request.Form[$"searchBuilder[criteria][{sbIndex}][value1]"].FirstOrDefault();
			var value1 = Request.Form[$"searchBuilder[criteria][{sbIndex}][value2]"].FirstOrDefault();

			if (!string.IsNullOrEmpty(origData) && !string.IsNullOrEmpty(condition))
			{
				if (origData == "name")
				{
					var val = value0 ?? "";
					query = condition switch
					{
						"=" => query.Where(mp => EF.Functions.ILike(mp.Supplier.Name, val)),
						"!=" => query.Where(mp => !EF.Functions.ILike(mp.Supplier.Name, val)),
						"starts" => query.Where(mp => EF.Functions.ILike(mp.Supplier.Name, $"{val}%")),
						"!starts" => query.Where(mp => !EF.Functions.ILike(mp.Supplier.Name, $"{val}%")),
						"contains" => query.Where(mp => EF.Functions.ILike(mp.Supplier.Name, $"%{val}%")),
						"!contains" => query.Where(mp => !EF.Functions.ILike(mp.Supplier.Name, $"%{val}%")),
						"ends" => query.Where(mp => EF.Functions.ILike(mp.Supplier.Name, $"%{val}")),
						"!ends" => query.Where(mp => !EF.Functions.ILike(mp.Supplier.Name, $"%{val}")),
						"null" => query.Where(mp => string.IsNullOrEmpty(mp.Supplier.Name)),
						"!null" => query.Where(mp => !string.IsNullOrEmpty(mp.Supplier.Name)),
						_ => query
					};
				}
				else if (origData == "total" || origData == "tax" || origData == "discount_value" || origData == "amount")
				{
					decimal.TryParse(value0, out var d0);
					decimal.TryParse(value1, out var d1);

					if (origData == "total")
					{
						query = condition switch
						{
							"=" => query.Where(mp => mp.Total == d0),
							"!=" => query.Where(mp => mp.Total != d0),
							"<" => query.Where(mp => mp.Total < d0),
							"<=" => query.Where(mp => mp.Total <= d0),
							">" => query.Where(mp => mp.Total > d0),
							">=" => query.Where(mp => mp.Total >= d0),
							"between" => query.Where(mp => mp.Total >= d0 && mp.Total <= d1),
							"!between" => query.Where(mp => mp.Total < d0 || mp.Total > d1),
							_ => query
						};
					}
					else if (origData == "tax")
					{
						query = condition switch
						{
							"=" => query.Where(mp => mp.Tax == d0),
							"!=" => query.Where(mp => mp.Tax != d0),
							"<" => query.Where(mp => mp.Tax < d0),
							"<=" => query.Where(mp => mp.Tax <= d0),
							">" => query.Where(mp => mp.Tax > d0),
							">=" => query.Where(mp => mp.Tax >= d0),
							"between" => query.Where(mp => mp.Tax >= d0 && mp.Tax <= d1),
							"!between" => query.Where(mp => mp.Tax < d0 || mp.Tax > d1),
							_ => query
						};
					}
					else if (origData == "discount_value")
					{
						query = condition switch
						{
							"=" => query.Where(mp => mp.DiscountValue == d0),
							"!=" => query.Where(mp => mp.DiscountValue != d0),
							"<" => query.Where(mp => mp.DiscountValue < d0),
							"<=" => query.Where(mp => mp.DiscountValue <= d0),
							">" => query.Where(mp => mp.DiscountValue > d0),
							">=" => query.Where(mp => mp.DiscountValue >= d0),
							"between" => query.Where(mp => mp.DiscountValue >= d0 && mp.DiscountValue <= d1),
							"!between" => query.Where(mp => mp.DiscountValue < d0 || mp.DiscountValue > d1),
							_ => query
						};
					}
					else if (origData == "amount")
					{
						query = condition switch
						{
							"=" => query.Where(mp => mp.Amount == d0),
							"!=" => query.Where(mp => mp.Amount != d0),
							"<" => query.Where(mp => mp.Amount < d0),
							"<=" => query.Where(mp => mp.Amount <= d0),
							">" => query.Where(mp => mp.Amount > d0),
							">=" => query.Where(mp => mp.Amount >= d0),
							"between" => query.Where(mp => mp.Amount >= d0 && mp.Amount <= d1),
							"!between" => query.Where(mp => mp.Amount < d0 || mp.Amount > d1),
							_ => query
						};
					}
				}
				else if (origData == "date")
				{
					if (DateTime.TryParse(value0, out var dt0))
					{
						var nextDay = dt0.Date.AddDays(1);
						DateTime dt1;
						
						switch (condition)
						{
							case "=":
							case "contains":
								query = query.Where(mp => mp.Date >= dt0.Date && mp.Date < nextDay);
								break;
							case "!=":
							case "!contains":
								query = query.Where(mp => mp.Date < dt0.Date || mp.Date >= nextDay);
								break;
							case "<":
								query = query.Where(mp => mp.Date < dt0.Date);
								break;
							case "<=":
								query = query.Where(mp => mp.Date < nextDay);
								break;
							case ">":
								query = query.Where(mp => mp.Date >= nextDay);
								break;
							case ">=":
								query = query.Where(mp => mp.Date >= dt0.Date);
								break;
							case "between":
								if (DateTime.TryParse(value1, out dt1))
								{
									var endDay = dt1.Date.AddDays(1);
									query = query.Where(mp => mp.Date >= dt0.Date && mp.Date < endDay);
								}
								break;
							case "!between":
								if (DateTime.TryParse(value1, out dt1))
								{
									var endDay = dt1.Date.AddDays(1);
									query = query.Where(mp => mp.Date < dt0.Date || mp.Date >= endDay);
								}
								break;
						}
					}
				}
			}
			sbIndex++;
		}

		// Filtered count
		var filteredRecords = await query.CountAsync();

		// Apply sorting
		if (!string.IsNullOrEmpty(sortColumnIndex))
		{
			switch (sortColumnIndex)
			{
				case "1":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.Supplier.Name) 
						: query.OrderBy(mp => mp.Supplier.Name);
					break;
				case "2":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.Total) 
						: query.OrderBy(mp => mp.Total);
					break;
				case "3":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.Tax) 
						: query.OrderBy(mp => mp.Tax);
					break;
				case "4":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.DiscountValue) 
						: query.OrderBy(mp => mp.DiscountValue);
					break;
				case "5":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.Amount) 
						: query.OrderBy(mp => mp.Amount);
					break;
				case "6":
					query = sortDirection == "desc" 
						? query.OrderByDescending(mp => mp.Date) 
						: query.OrderBy(mp => mp.Date);
					break;
				default:
					query = query.OrderByDescending(mp => mp.CreatedDate);
					break;
			}
		}
		else
		{
			query = query.OrderByDescending(mp => mp.CreatedDate);
		}

		// Pagination
		var pagedQuery = query.Skip(start);
		if (length > 0)
		{
			pagedQuery = pagedQuery.Take(length);
		}

		var rawData = await pagedQuery.ToListAsync();

		var formattedData = rawData.Select(mp => new
		{
			id = mp.Id,
			name = mp.Supplier.Name,
			total = "Rs " + mp.Total.ToString("F2"),
			tax = "Rs " + mp.Tax.ToString("F2"),
			discount_value = "Rs " + mp.DiscountValue.ToString("F2"),
			amount = "Rs " + mp.Amount.ToString("F2"),
			date = mp.Date.ToString("dd/MM/yyyy"),
			action = $@"
				<div class=""dropdown d-inline-block"">
					<a class=""text-primary edit dropdown-toggle"" data-toggle=""dropdown""><i class=""las la-ellipsis-h""></i></a>
					<ul class=""dropdown-menu dropdown-menu-right"">
						<li><a href=""/Medicines/Purchase/View/{mp.Id}""><i class=""las la-laptop pr-2""></i>View</a></li>
						<li><a href=""/Medicines/Purchase/Edit/{mp.Id}""><i class=""las la-edit pr-2""></i>Edit</a></li>
					</ul>
				</div>
				<a class=""table-delete text-danger delete"" data-toggle=""tooltip"" title=""Delete"">
					<i class=""las la-trash-alt""></i>
					<input type=""hidden"" value=""{mp.Id}"">
				</a>"
		}).ToList();

		return Json(new
		{
			draw = draw,
			recordsTotal = totalRecords,
			recordsFiltered = filteredRecords,
			data = formattedData
		});
	}
}
