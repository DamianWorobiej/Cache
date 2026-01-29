using Cache;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
    const string redisKey = "forecasts";
    var redisDb = RedisDbFactory.GetDatabase();
    
    if (redisDb.KeyExists(redisKey))
    {
        var redisValue = await redisDb.StringGetAsync(redisKey);
        var returnedData = JsonSerializer.Deserialize<List<WeatherForecast>>(redisValue.ToString());

        return returnedData;
    }

    var forecasts = await context.Forecasts.ToListAsync();

    var cachedData = JsonSerializer.Serialize(forecasts);
    await redisDb.StringSetAsync(redisKey, cachedData, new TimeSpan(0, 0, 10));

    return forecasts;
});

app.MapGet("/forecastsahead/{days}", async (WeatherDbContext context, int days) =>
{
    const string redisKey = "forecastsSet";

    var redisDb = RedisDbFactory.GetDatabase();
    var isCached = true;
    var cachedOutput = new List<WeatherForecast>();
    foreach (var index in Enumerable.Range(0, days + 1))
    {
        var key = $"{redisKey}:{DateOnly.FromDateTime(DateTime.Now.AddDays(index))}";
        if (await redisDb.KeyExistsAsync(key) is false)
        {
            isCached = false;

            break;
        }

        var cachedValue = await redisDb.StringGetAsync(key);
        cachedOutput.Add(JsonSerializer.Deserialize<WeatherForecast>(cachedValue.ToString()));
    }

    if (isCached)
    {
        return cachedOutput;
    }

    var forecasts = await context.Forecasts
    .OrderBy(x => x.Date)
    .Take(days + 1)
    .ToListAsync();

    foreach (var forecast in forecasts)
    {
        var date = forecast.Date.ToString();
        await redisDb.StringSetAsync($"{redisKey}:{date}", JsonSerializer.Serialize(forecast), new TimeSpan(0, 0, 10));
    }

    return forecasts;
});

app.Run();
