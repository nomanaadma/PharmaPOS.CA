using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaPOS_MVC.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseNpgsql(connectionString);
	options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

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

app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
		Path.Combine(app.Environment.WebRootPath, "uploads")),
	RequestPath = "/public/uploads"
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Dashboard}/{action=Index}/{id?}")
	.WithStaticAssets();

app.MapRazorPages()
	.WithStaticAssets();

app.Run();