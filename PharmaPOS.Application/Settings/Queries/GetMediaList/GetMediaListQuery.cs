using MediatR;
using PharmaPOS.Domain.MediaAggregate;

namespace PharmaPOS.Application.Settings.Queries.GetMediaList;

public record GetMediaListQuery() : IRequest<List<Media>>;
