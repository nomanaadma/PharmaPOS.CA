using MediatR;
using PharmaPOS.Application.Interfaces;

namespace PharmaPOS.Application.Medicines.Queries.SearchMedicines;

public class SearchMedicinesQueryHandler(
	IMedicineRepository medicineRepo) : IRequestHandler<SearchMedicinesQuery, object>
{
	public async Task<object> Handle(SearchMedicinesQuery query, CancellationToken ct)
	{
		return await medicineRepo.SearchMedicinesAsync(query.Term, 10, ct);
	}
}
