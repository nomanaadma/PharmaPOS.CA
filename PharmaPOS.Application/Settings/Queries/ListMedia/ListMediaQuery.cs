using MediatR;
using PharmaPOS.Domain.MediaAggregate;

namespace PharmaPOS.Application.Settings.Queries.ListMedia;

public record ListMediaQuery() : IRequest<List<Media>>;
