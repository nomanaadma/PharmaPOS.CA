using PharmaPOS.Contracts.Responses;
using PharmaPOS.Domain.MedicineAggregate;

namespace PharmaPOS.Application.Interfaces;

public interface IMedicineRepository : IRepository<Medicine>
{
	Task<MedicineDetailsResponse?> GetMedicineDetailsAsync(int id, CancellationToken ct = default);
	Task<MedicinesWithStockResponse> GetMedicinesWithStockAsync(CancellationToken ct = default);
	Task<Dictionary<int, string>> GetMedicineNamesByIdsAsync(List<int> medicineIds, CancellationToken ct = default);
	Task<List<MedicineSearchResult>> SearchMedicinesAsync(string term, int take = 10, CancellationToken ct = default);
}
