namespace Global_Insights_Dashboard.Models.Configuration;

/// <summary>
/// Application settings and user preferences
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Theme settings
    /// </summary>
    public ThemeSettings Theme { get; set; } = new();

    /// <summary>
    /// Auto-refresh settings
    /// </summary>
    public RefreshSettings Refresh { get; set; } = new();

    /// <summary>
    /// User preferences
    /// </summary>
    public UserPreferences Preferences { get; set; } = new();

    /// <summary>
    /// Cache settings
    /// </summary>
    public CacheSettings Cache { get; set; } = new();
}

public class ThemeSettings
{
    public bool IsDarkMode { get; set; } = false;
    public string PrimaryColor { get; set; } = "DeepPurple";
    public string SecondaryColor { get; set; } = "Lime";
}

public class RefreshSettings
{
    public bool AutoRefreshEnabled { get; set; } = true;
    public int RefreshIntervalMinutes { get; set; } = 15;
    public DateTime LastRefresh { get; set; } = DateTime.MinValue;
}

public class UserPreferences
{
    public string LastWeatherCity { get; set; } = "London";
    public string LastWeatherCountry { get; set; } = "GB";
    public string NewsCountry { get; set; } = "us";
    public string NewsCategory { get; set; } = "general";
    public List<string> FavoriteStockSymbols { get; set; } = new() { "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA" };
    public string LastActiveService { get; set; } = "Weather";
}

public class CacheSettings
{
    public int CacheExpirationMinutes { get; set; } = 30;
    public bool EnableOfflineMode { get; set; } = true;
    public int MaxCacheSize { get; set; } = 100; // Maximum number of cached items per service
}
