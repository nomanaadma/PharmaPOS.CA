using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Billings.Queries.GetRecentBills;

public record GetRecentBillsQuery(int CustomerId, string? Email) : IRequest<List<CustomerBillsResponse>>;
