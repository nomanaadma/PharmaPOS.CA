namespace PharmaPOS.Contracts.Responses;

/// <summary>
/// The result returned by <see cref="DataTableHelper.FilterAsync{T}"/>,
/// containing counts and the paged data ready for JSON serialization.
/// </summary>
public class DataTableResult<T> where T : class
{
	public required int TotalRecords { get; init; }
	public required int FilteredRecords { get; init; }
	public required List<T> Data { get; init; }
}
