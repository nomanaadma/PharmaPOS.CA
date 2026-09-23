using MediatR;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Domain.MediaAggregate;

namespace PharmaPOS.Application.Settings.Queries.GetMediaList;

public class GetMediaListQueryHandler(
	IMediaRepository mediaRepo) : IRequestHandler<GetMediaListQuery, List<Media>>
{
	public async Task<List<Media>> Handle(GetMediaListQuery query, CancellationToken ct)
	{
		return await mediaRepo.GetMediaListAsync(ct);
	}
}
