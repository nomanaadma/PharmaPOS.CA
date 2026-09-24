using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Billings.Queries.ListRecentBills;

public record ListRecentBillsQuery(int CustomerId, string? Email) : IRequest<List<CustomerBillsResponse>>;
