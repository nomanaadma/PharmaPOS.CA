using System.Diagnostics;
using MediatR;
using PharmaPOS.Application.Common.Metrics;

namespace PharmaPOS.Application.Common.Behaviors;

public class PerformanceMetricBehavior<TRequest, TResponse>(
	PerformanceMetric metric)
	: IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var requestName = typeof(TRequest).Name;
		var stopwatch = Stopwatch.StartNew();
        
		try
		{
			return await next(cancellationToken);
		}
		finally
		{
			stopwatch.Stop();
			metric.RequestDuration.Record(stopwatch.Elapsed.TotalMilliseconds, 
				new KeyValuePair<string, object?>("request_name", requestName));
		}
	}
}