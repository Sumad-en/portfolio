
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register Portfolio Repository
builder.Services.AddScoped<portfolio.Data.PortfolioRepository>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
