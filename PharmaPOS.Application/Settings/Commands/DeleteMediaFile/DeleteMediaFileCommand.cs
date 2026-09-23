using ErrorOr;
using MediatR;

namespace PharmaPOS.Application.Settings.Commands.DeleteMediaFile;

public record DeleteMediaFileCommand(int Id, string Name) : IRequest<ErrorOr<Deleted>>;
