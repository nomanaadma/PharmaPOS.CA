using MediatR;

namespace PharmaPOS.Application.Medicines.Queries.SearchMedicines;

public record SearchMedicinesQuery(string Term) : IRequest<object>;
