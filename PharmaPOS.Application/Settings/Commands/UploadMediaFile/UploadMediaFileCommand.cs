using ErrorOr;
using MediatR;
using PharmaPOS.Contracts.Responses;

namespace PharmaPOS.Application.Settings.Commands.UploadMediaFile;

public record UploadMediaFileCommand(Stream FileStream, string OriginalFileName, long Length) : IRequest<ErrorOr<MediaUploadResponse>>;
