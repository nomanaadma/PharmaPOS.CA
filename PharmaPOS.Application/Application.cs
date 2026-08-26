using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PharmaPOS.Application.Data;
using PharmaPOS.Application.Services;
using PharmaPOS.MVC.Services;

namespace PharmaPOS.Application;

public static class ApplicationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
	    public void AddApplication()
	    {
	    
		    services.AddScoped<SettingService>();
		    services.AddScoped<BillingService>();
		    services.AddScoped<CustomerService>();
		    services.AddScoped<PurchaseService>();
		    services.AddScoped<PaymentMethodService>();
		    services.AddScoped<SupplierService>();
		    services.AddScoped<MedicineService>();
		    services.AddScoped<CategoryService>();
		    services.AddScoped<StockService>();
		    services.AddScoped<ReportService>();
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