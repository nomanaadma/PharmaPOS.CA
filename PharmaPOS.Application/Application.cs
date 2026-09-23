using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using PharmaPOS.Application.Mappers;

namespace PharmaPOS.Application;

public static class ApplicationServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		// Register MediatR handlers from this assembly
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
		
		var config = TypeAdapterConfig.GlobalSettings;
		BillMapper.Register(config);
		BatchMapper.Register(config);
		PurchaseMapper.Register(config);
		
		return services;
	}
}