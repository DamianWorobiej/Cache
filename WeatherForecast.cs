namespace Cache;

public record WeatherForecast(DateOnly Date, int Temperature, string? Summary)
{
    public int Id { get; set; }
}