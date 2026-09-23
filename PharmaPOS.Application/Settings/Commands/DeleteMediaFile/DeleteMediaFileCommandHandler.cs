using ErrorOr;
using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Application.IProviders;

namespace PharmaPOS.Application.Settings.Commands.DeleteMediaFile;

public class DeleteMediaFileCommandHandler(
	IMediaRepository mediaRepo,
	IUnitOfWork unitOfWork,
	IFilePathProvider pathProvider) : IRequestHandler<DeleteMediaFileCommand, ErrorOr<Deleted>>
{
	public async Task<ErrorOr<Deleted>> Handle(DeleteMediaFileCommand command, CancellationToken ct)
	{
		var media = await mediaRepo.GetByIdAndFilenameAsync(command.Id, command.Name, ct);
		if (media == null)
			return Error.NotFound(description: "File is wrong or does not exist.");

		var filePath = pathProvider.GetUploadsFilePath(command.Name);
		if (File.Exists(filePath))
		{
			try
			{
				File.Delete(filePath);
			}
			catch (Exception ex)
			{
				return Error.Failure(description: "Error deleting " + command.Name + ": " + ex.Message);
			}
		}

		mediaRepo.Remove(media);
		await unitOfWork.SaveChangesAsync(ct);

		return Result.Deleted;
	}
}
