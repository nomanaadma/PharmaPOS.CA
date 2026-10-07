using MediatR;
using Microsoft.Extensions.Logging;

namespace PharmaPOS.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Processing MediatR Request: {RequestName} {@Request}", requestName, request);

        var response = await next(cancellationToken);

        logger.LogInformation("Completed MediatR Request: {RequestName}", requestName);

        return response;
    }
}
