using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Medicines.Queries.ListMedicinesWithStock;

public record ListMedicinesWithStockQuery() : IRequest<List<MedicineResponse>>;
