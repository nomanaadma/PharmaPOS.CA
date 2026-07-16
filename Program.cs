using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;
using PharmaPOS_MVC.Middlewares;
using PharmaPOS_MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseNpgsql(connectionString);
	options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
	options.EnableSensitiveDataLogging();
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<SiteInfoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
	
	// Auto-seed on startup
	await DbInitializer.SeedAuthData(app.Services);
	
}
else
{
	app.UseExceptionHandler("/Dashboard/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseMiddleware<BlockRegisterMiddleware>();

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
		Path.Combine(app.Environment.WebRootPath, "uploads")),
	RequestPath = "/uploads"
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Dashboard}/{action=Index}/{id?}")
	.WithStaticAssets();

app.MapRazorPages()
	.WithStaticAssets();

var defaultCulture = new CultureInfo("en-GB"); // en-GB uses dd/MM/yyyy naturally
var localizationOptions = new RequestLocalizationOptions
{
	DefaultRequestCulture = new RequestCulture(defaultCulture),
	SupportedCultures = new List<CultureInfo> { defaultCulture },
	SupportedUICultures = new List<CultureInfo> { defaultCulture }
};
app.UseRequestLocalization(localizationOptions);

app.Run();