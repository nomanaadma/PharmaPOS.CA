using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Stock.Queries.ListStock;

public record ListStockQuery(string? Type) : IRequest<List<BatchResponse>>;
