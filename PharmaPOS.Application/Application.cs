using Microsoft.Extensions.DependencyInjection;
using PharmaPOS.Application.Services;

namespace PharmaPOS.Application;

public static class ApplicationServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		var assembly = typeof(IApplicationMarker).Assembly;

		var scopedServices = assembly.GetTypes()
			.Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IApplicationService).IsAssignableFrom(t));

		foreach (var type in scopedServices)
		{
			services.AddScoped(type);
		}

		return services;
	}
}