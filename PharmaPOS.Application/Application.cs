using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PharmaPOS.Application.Data;
using PharmaPOS.Application.Services;

namespace PharmaPOS.Application;

public static class ApplicationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
	    public void AddApplication()
	    {
		    
		    var assembly = typeof(IApplicationMarker).Assembly;
		    
		    var scopedServices = assembly.GetTypes()
			    .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IApplicationService).IsAssignableFrom(t));
		    
		    foreach (var type in scopedServices)
		    {
			    services.AddScoped(type);
		    }
		    
	    }

	    public void AddDatabase(string? connectionString)
	    {
	    
		    connectionString = connectionString ??
		                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		
		    services.AddDbContext<ApplicationDbContext>(options =>
		    {
			    options.UseNpgsql(connectionString);
			    options.EnableSensitiveDataLogging();
		    });
	    
		    services.AddScoped<DbInitializer>();
	    }
    }
}