using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using PharmaPOS.Application.Services;

namespace PharmaPOS.Application;

public static class ApplicationServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly(); 
		
		var scopedServices = assembly.GetTypes()
			.Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IApplicationService).IsAssignableFrom(t));

		foreach (var type in scopedServices)
		{
			services.AddScoped(type);
		}

		// Register MediatR handlers from this assembly
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
		
		var config = TypeAdapterConfig.GlobalSettings;
		config.Scan(assembly);
		
		return services;
	}
}