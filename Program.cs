using Cache;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WeatherDbContext>(options => options.UseInMemoryDatabase("cacheDb"));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<WeatherDbContext>();
    Seeder.Seed(context);
}

app.MapGet("/weatherforecast", async (WeatherDbContext context) =>
{
    var forecast = await context.Forecasts.ToListAsync();

    return forecast;
});

app.MapGet("/forecastsahead/{days}", async (WeatherDbContext context, int days) =>
{
    var forecast = await context.Forecasts
    .OrderBy(x => x.Date)
    .Take(days + 1)
    .ToListAsync();

    return forecast;
});

app.Run();
