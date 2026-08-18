namespace PharmaPOS_MVC.Helpers;

/// <summary>
/// Represents a single SearchBuilder criterion parsed from the DataTables form POST.
/// </summary>
public class SearchCriteria
{
	public string OrigData { get; init; } = "";
	public string Condition { get; init; } = "";
	public string Type { get; set; } = "";
	public string? Value0 { get; init; }
	public string? Value1 { get; init; }
}

/// <summary>
/// Encapsulates all DataTables server-side parameters extracted from Request.Form.
/// Replaces the duplicated parameter-parsing code at the top of every Filter action.
/// </summary>
public class DataTableRequest
{
	public int Draw { get; private init; }
	public int Start { get; private init; }
	public int Length { get; private init; }
	public string? SearchValue { get; private init; }
	public string? SortColumnIndex { get; private init; }
	public string SortDirection { get; private init; } = "asc";
	public List<SearchCriteria> SearchBuilderCriteria { get; set; } = [];

	/// <summary>
	/// Parses a DataTables server-side POST into a strongly-typed request object.
	/// </summary>
	public static DataTableRequest FromForm(IFormCollection form)
	{
		var request = new DataTableRequest
		{
			Draw = Convert.ToInt32(form["draw"].FirstOrDefault() ?? "1"),
			Start = Convert.ToInt32(form["start"].FirstOrDefault() ?? "0"),
			Length = Convert.ToInt32(form["length"].FirstOrDefault() ?? "10"),
			SearchValue = form["search[value]"].FirstOrDefault(),
			SortColumnIndex = form["order[0][column]"].FirstOrDefault(),
			SortDirection = form["order[0][dir]"].FirstOrDefault() ?? "asc"
		};

		// Parse SearchBuilder criteria
		var sbIndex = 0;
		while (form.ContainsKey($"searchBuilder[criteria][{sbIndex}][condition]"))
		{
			var origData = form[$"searchBuilder[criteria][{sbIndex}][origData]"].FirstOrDefault();
			var condition = form[$"searchBuilder[criteria][{sbIndex}][condition]"].FirstOrDefault();

			if (!string.IsNullOrEmpty(origData) && !string.IsNullOrEmpty(condition))
			{
				request.SearchBuilderCriteria.Add(new SearchCriteria
				{
					OrigData = origData,
					Condition = condition,
					Type = form[$"searchBuilder[criteria][{sbIndex}][type]"].FirstOrDefault() ?? "",
					Value0 = form[$"searchBuilder[criteria][{sbIndex}][value1]"].FirstOrDefault(),
					Value1 = form[$"searchBuilder[criteria][{sbIndex}][value2]"].FirstOrDefault()
				});
			}

			sbIndex++;
		}

		return request;
	}
}
