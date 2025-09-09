using Global_Insights_Dashboard.Models.DTOs;

namespace Global_Insights_Dashboard.Services.Interfaces;

/// <summary>
/// Service for weather data retrieval from OpenWeatherMap API
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Get current weather for a specific city
    /// </summary>
    /// <param name="city">City name</param>
    /// <param name="country">Country code (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current weather data</returns>
    Task<CurrentWeatherResponse?> GetCurrentWeatherAsync(string city, string? country = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get 5-day weather forecast for a specific city
    /// </summary>
    /// <param name="city">City name</param>
    /// <param name="country">Country code (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>5-day weather forecast</returns>
    Task<WeatherForecastResponse?> GetForecastAsync(string city, string? country = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current weather by coordinates
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current weather data</returns>
    Task<CurrentWeatherResponse?> GetCurrentWeatherByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get forecast by coordinates
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>5-day weather forecast</returns>
    Task<WeatherForecastResponse?> GetForecastByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test API connectivity and key validity
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if API is accessible</returns>
    Task<bool> TestApiConnectivityAsync(CancellationToken cancellationToken = default);
}
