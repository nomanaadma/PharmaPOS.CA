using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using PharmaPOS.Application;
using PharmaPOS.Application.Authorization;
using PharmaPOS.Application.Data;
using PharmaPOS.MVC.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabase(builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
	foreach (var perm in Permissions.GetAll())
	{
		options.AddPolicy(perm, policy =>
			policy.RequireAssertion(ctx =>
				ctx.User.HasClaim(c => c.Type == "Permission" && c.Value == perm)));
	}
});

builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
	
	using var scope = app.Services.CreateScope();
	var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
	await dbInitializer.SeedAuthDataAsync();
	
}
else
{
	app.UseExceptionHandler("/Dashboard/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Dashboard/Status", "?code={0}");
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