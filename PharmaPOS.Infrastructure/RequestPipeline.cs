using Microsoft.AspNetCore.Builder;
using PharmaPOS.Infrastructure.Common;

namespace PharmaPOS.Infrastructure;


public static class RequestPipeline
{
	public static IApplicationBuilder AddInfrastructureMiddleware(this IApplicationBuilder builder)
	{
		builder.UseMiddleware<EventualConsistencyMiddleware>();

		return builder;
	}
}