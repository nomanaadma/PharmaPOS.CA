using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Billings.Queries.GetBillForEdit;

public record GetBillForEditQuery(int Id) : IRequest<ErrorOr<BillEditResponse>>;
