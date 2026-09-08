using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PharmaPOS.Application.Interfaces;
using PharmaPOS.Infrastructure.Data;
using PharmaPOS.Infrastructure.Repositories;

namespace PharmaPOS.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
	{
		if (string.IsNullOrEmpty(connectionString))
		{
			throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		}

		services.AddDbContext<ApplicationDbContext>(options =>
		{
			options.UseNpgsql(connectionString, b => 
				b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
			options.EnableSensitiveDataLogging();
		});

		services.AddScoped<DbInitializer>();
		services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
		services.AddScoped<IUnitOfWork, UnitOfWork>();

		return services;
	}
}
