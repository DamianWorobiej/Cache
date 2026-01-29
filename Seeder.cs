namespace Cache;

public static class Seeder
{
    private static int ForecastsCount = 60;
    private static string[] Summaries =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    ];

    public static void Seed(WeatherDbContext context)
    {
        var forecasts = new List<WeatherForecast>();
        for (int i = 0; i < ForecastsCount; i++)
        {
            forecasts.Add(RollForecast(i));
        }

        context.Forecasts.AddRange(forecasts);
        context.SaveChanges();
    }

    private static WeatherForecast RollForecast(int index) =>
        new
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            Summaries[Random.Shared.Next(Summaries.Length)]
        );
}
