using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Billings.Commands.CreateBill;

public record CreateBillCommand(BillingRequest Request) : IRequest<ErrorOr<int>>;
