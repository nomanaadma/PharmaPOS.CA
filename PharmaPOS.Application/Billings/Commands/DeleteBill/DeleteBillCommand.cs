using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.Billings.Commands.DeleteBill;

public record DeleteBillCommand(int Id) : IRequest<ErrorOr<Deleted>>;
