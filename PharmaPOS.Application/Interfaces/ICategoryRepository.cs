using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.MedicineCategoryAggregate;

namespace PharmaPOS.Application.Interfaces;

public interface ICategoryRepository : IRepository<MedicineCategory>
{
	Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default);
	Task<List<LookupItem>> GetLookupListAsync(CancellationToken ct = default);

	Task<string?> GetCategoryNameByIdAsync(int id, CancellationToken ct = default);

	Task<Dictionary<int, string>> GetCategoryNamesByIdsAsync(List<int> categoryIds, CancellationToken ct = default);

}


