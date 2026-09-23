using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Requests;

namespace PharmaPOS.Application.Billings.Commands.UpdateBill;

public record UpdateBillCommand(int Id, BillingRequest Request) : IRequest<ErrorOr<Updated>>;
