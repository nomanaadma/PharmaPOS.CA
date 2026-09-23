using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Stock.Queries.GetStockList;

public record GetStockListQuery(string? Type) : IRequest<List<BatchResponse>>;
