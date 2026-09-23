using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Billings.Queries.GetBill;

public record GetBillQuery(int Id) : IRequest<ErrorOr<BillResponse>>;
