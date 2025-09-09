namespace Global_Insights_Dashboard.Models.Configuration;

/// <summary>
/// Configuration model for API keys and endpoints
/// </summary>
public class ApiConfiguration
{
    /// <summary>
    /// OpenWeatherMap API configuration
    /// </summary>
    public WeatherApiConfig Weather { get; set; } = new();

    /// <summary>
    /// NewsAPI configuration
    /// </summary>
    public NewsApiConfig News { get; set; } = new();

    /// <summary>
    /// Alpha Vantage API configuration
    /// </summary>
    public FinanceApiConfig Finance { get; set; } = new();

    /// <summary>
    /// Open Trivia Database API configuration
    /// </summary>
    public TriviaApiConfig Trivia { get; set; } = new();
}

public class WeatherApiConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5";
    public string Units { get; set; } = "metric"; // metric, imperial, kelvin
}

public class NewsApiConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://newsapi.org/v2";
    public int PageSize { get; set; } = 20;
}

public class FinanceApiConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://www.alphavantage.co/query";
    public int RequestDelayMs { get; set; } = 12000; // 5 requests per minute limit
}

public class TriviaApiConfig
{
    public string BaseUrl { get; set; } = "https://opentdb.com/api.php";
    // No API key required for Open Trivia Database
}
