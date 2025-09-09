using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Global_Insights_Dashboard.Models.DTOs;
using Global_Insights_Dashboard.Services.Interfaces;

namespace Global_Insights_Dashboard.Services;

/// <summary>
/// Service for exporting application data to CSV and other formats
/// </summary>
public class ExportService : IExportService
{
    public async Task<bool> ExportWeatherDataAsync(CurrentWeatherResponse? currentWeather, WeatherForecastResponse? forecast, string filePath)
    {
        try
        {
            var csv = new StringBuilder();
            
            // Header for weather data
            csv.AppendLine("Data Type,Location,Date/Time,Temperature (°F),Feels Like (°F),Humidity (%),Pressure (hPa),Wind Speed (mph),Wind Direction (°),Visibility (m),Weather Description,Icon");
            
            // Current weather data
            if (currentWeather != null)
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(currentWeather.Timestamp).ToString("yyyy-MM-dd HH:mm:ss");
                csv.AppendLine($"Current Weather,\"{currentWeather.Name}, {currentWeather.System?.Country}\"," +
                              $"{dateTime}," +
                              $"{currentWeather.Main?.Temperature:F1}," +
                              $"{currentWeather.Main?.FeelsLike:F1}," +
                              $"{currentWeather.Main?.Humidity}," +
                              $"{currentWeather.Main?.Pressure}," +
                              $"{currentWeather.Wind?.Speed:F1}," +
                              $"{currentWeather.Wind?.Direction}," +
                              $"{currentWeather.Visibility}," +
                              $"\"{currentWeather.Weather.FirstOrDefault()?.Description}\"," +
                              $"{currentWeather.Weather.FirstOrDefault()?.Icon}");
            }
            
            // Forecast data
            if (forecast?.Items != null)
            {
                foreach (var item in forecast.Items)
                {
                    var dateTime = item.DateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    csv.AppendLine($"Forecast,\"{forecast.City?.Name}, {forecast.City?.Country}\"," +
                                  $"{dateTime}," +
                                  $"{item.Main?.Temperature:F1}," +
                                  $"{item.Main?.FeelsLike:F1}," +
                                  $"{item.Main?.Humidity}," +
                                  $"{item.Main?.Pressure}," +
                                  $"{item.Wind?.Speed:F1}," +
                                  $"{item.Wind?.Direction}," +
                                  $"{item.Visibility}," +
                                  $"\"{item.Weather.FirstOrDefault()?.Description}\"," +
                                  $"{item.Weather.FirstOrDefault()?.Icon}");
                }
            }
            
            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ExportNewsDataAsync(NewsResponse? newsData, string filePath)
    {
        try
        {
            var csv = new StringBuilder();
            
            // Header for news data
            csv.AppendLine("Title,Author,Source,Published Date,Description,URL,URL to Image,Content");
            
            if (newsData?.Articles != null)
            {
                foreach (var article in newsData.Articles)
                {
                var publishedAt = article.PublishedAt.ToString("yyyy-MM-dd HH:mm:ss");
                csv.AppendLine($"\"{EscapeCsvField(article.Title)}\"," +
                              $"\"{EscapeCsvField(article.Author)}\"," +
                              $"\"{EscapeCsvField(article.Source?.Name)}\"," +
                              $"{publishedAt}," +
                              $"\"{EscapeCsvField(article.Description)}\"," +
                              $"{article.Url}," +
                              $"{article.ImageUrl}," +
                              $"\"{EscapeCsvField(article.Content)}\"");
                }
            }
            
            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ExportFinanceDataAsync(StockQuoteResponse? stockData, CompanySearchResponse? searchResults, string filePath)
    {
        try
        {
            var csv = new StringBuilder();
            
            // Header for finance data
            csv.AppendLine("Data Type,Symbol,Name,Open,High,Low,Price,Previous Close,Change,Change Percent,Volume,Market Cap,Exchange,Currency,Last Updated");
            
            // Stock quote data
            if (stockData?.Quote != null)
            {
                var quote = stockData.Quote;
                csv.AppendLine($"Stock Quote," +
                              $"{quote.Symbol}," +
                              $"\"{EscapeCsvField(quote.Symbol)}\"," +
                              $"{quote.Open:F2}," +
                              $"{quote.High:F2}," +
                              $"{quote.Low:F2}," +
                              $"{quote.Price:F2}," +
                              $"{quote.PreviousClose:F2}," +
                              $"{quote.Change:F2}," +
                              $"{quote.ChangePercent:F2}," +
                              $"{quote.Volume}," +
                              $"," + // Market Cap not available in current model
                              $"," + // Exchange not available in current model
                              $"," + // Currency not available in current model
                              $"{quote.LatestTradingDay}");
            }
            
            // Search results
            if (searchResults?.BestMatches != null)
            {
                foreach (var match in searchResults.BestMatches)
                {
                    csv.AppendLine($"Search Result," +
                                  $"{match.Symbol}," +
                                  $"\"{EscapeCsvField(match.Name)}\"," +
                                  $",,,,,,,,," + // Empty fields for price data
                                  $"{match.MatchScore}," +
                                  $"\"{EscapeCsvField(match.Type)}\"," +
                                  $"\"{EscapeCsvField(match.Region)}\"," +
                                  $"\"{EscapeCsvField(match.Currency)}\"");
                }
            }
            
            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ExportTriviaDataAsync(List<TriviaQuestion>? questions, List<TriviaUserAnswer>? answers, string filePath)
    {
        try
        {
            var csv = new StringBuilder();
            
            // Header for trivia data
            csv.AppendLine("Question,Category,Type,Difficulty,Correct Answer,Incorrect Answers,User Answer,Is Correct,Answered At");
            
            if (questions != null)
            {
                for (int i = 0; i < questions.Count; i++)
                {
                    var question = questions[i];
                    var userAnswer = answers != null && i < answers.Count ? answers[i] : null;
                    
                    var incorrectAnswers = string.Join("; ", question.IncorrectAnswers ?? new List<string>());
                    
                    csv.AppendLine($"\"{EscapeCsvField(question.Question)}\"," +
                                  $"\"{EscapeCsvField(question.Category)}\"," +
                                  $"{question.Type}," +
                                  $"{question.Difficulty}," +
                                  $"\"{EscapeCsvField(question.CorrectAnswer)}\"," +
                                  $"\"{EscapeCsvField(incorrectAnswers)}\"," +
                                  $"\"{EscapeCsvField(userAnswer?.UserAnswer)}\"," +
                                  $"{userAnswer?.IsCorrect}," +
                                  $"{userAnswer?.AnsweredAt:yyyy-MM-dd HH:mm:ss}");
                }
            }
            
            await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string? ShowSaveFileDialog(string defaultFileName, string filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*")
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = filter,
                DefaultExt = "csv",
                AddExtension = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Escape CSV field content to handle commas, quotes, and newlines
    /// </summary>
    private static string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // Replace double quotes with double double quotes and handle newlines
        return field.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
    }
}
