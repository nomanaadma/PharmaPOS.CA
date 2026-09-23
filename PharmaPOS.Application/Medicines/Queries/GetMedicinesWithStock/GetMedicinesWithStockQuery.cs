using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Medicines.Queries.GetMedicinesWithStock;

public record GetMedicinesWithStockQuery() : IRequest<List<MedicineResponse>>;
