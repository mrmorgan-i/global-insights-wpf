using Global_Insights_Dashboard.Models.DTOs;

namespace Global_Insights_Dashboard.Services.Interfaces;

/// <summary>
/// Service for exporting data to various formats
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export weather data to CSV format
    /// </summary>
    Task<bool> ExportWeatherDataAsync(CurrentWeatherResponse? currentWeather, WeatherForecastResponse? forecast, string filePath);

    /// <summary>
    /// Export news data to CSV format
    /// </summary>
    Task<bool> ExportNewsDataAsync(NewsResponse? newsData, string filePath);

    /// <summary>
    /// Export finance data to CSV format
    /// </summary>
    Task<bool> ExportFinanceDataAsync(StockQuoteResponse? stockData, CompanySearchResponse? searchResults, string filePath);

    /// <summary>
    /// Export trivia data to CSV format
    /// </summary>
    Task<bool> ExportTriviaDataAsync(List<TriviaQuestion>? questions, List<TriviaUserAnswer>? answers, string filePath);

    /// <summary>
    /// Show save file dialog and return selected path
    /// </summary>
    string? ShowSaveFileDialog(string defaultFileName, string filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*");
}

/// <summary>
/// Represents a user's answer to a trivia question for export
/// </summary>
public class TriviaUserAnswer
{
    public string Question { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; }
}
