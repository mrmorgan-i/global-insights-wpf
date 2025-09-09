using System.Net.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Global_Insights_Dashboard.Models.Configuration;
using Global_Insights_Dashboard.Models.DTOs;
using Global_Insights_Dashboard.Services.Interfaces;

namespace Global_Insights_Dashboard.Services;

/// <summary>
/// Implementation of weather service using OpenWeatherMap API
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguration _apiConfig;

    public WeatherService(HttpClient httpClient, IOptions<ApiConfiguration> apiConfig)
    {
        _httpClient = httpClient;
        _apiConfig = apiConfig.Value;
    }

    public async Task<CurrentWeatherResponse?> GetCurrentWeatherAsync(string city, string? country = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City name cannot be empty", nameof(city));

        var location = string.IsNullOrWhiteSpace(country) ? city : $"{city},{country}";
        var url = BuildCurrentWeatherUrl(location);

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var weatherData = JsonConvert.DeserializeObject<CurrentWeatherResponse>(response);
            
            return weatherData;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve weather data for {location}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException($"Weather API request timed out for {location}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse weather data for {location}: {ex.Message}", ex);
        }
    }

    public async Task<WeatherForecastResponse?> GetForecastAsync(string city, string? country = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City name cannot be empty", nameof(city));

        var location = string.IsNullOrWhiteSpace(country) ? city : $"{city},{country}";
        var url = BuildForecastUrl(location);

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var forecastData = JsonConvert.DeserializeObject<WeatherForecastResponse>(response);
            
            return forecastData;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve forecast data for {location}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException($"Forecast API request timed out for {location}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse forecast data for {location}: {ex.Message}", ex);
        }
    }

    public async Task<CurrentWeatherResponse?> GetCurrentWeatherByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var url = BuildCurrentWeatherUrlByCoordinates(latitude, longitude);

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var weatherData = JsonConvert.DeserializeObject<CurrentWeatherResponse>(response);
            
            return weatherData;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve weather data for coordinates ({latitude}, {longitude}): {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException($"Weather API request timed out for coordinates ({latitude}, {longitude})", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse weather data for coordinates ({latitude}, {longitude}): {ex.Message}", ex);
        }
    }

    public async Task<WeatherForecastResponse?> GetForecastByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var url = BuildForecastUrlByCoordinates(latitude, longitude);

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var forecastData = JsonConvert.DeserializeObject<WeatherForecastResponse>(response);
            
            return forecastData;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve forecast data for coordinates ({latitude}, {longitude}): {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException($"Forecast API request timed out for coordinates ({latitude}, {longitude})", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse forecast data for coordinates ({latitude}, {longitude}): {ex.Message}", ex);
        }
    }

    public async Task<bool> TestApiConnectivityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Test with a known good location
            var result = await GetCurrentWeatherAsync("London", "GB", cancellationToken);
            return result != null && result.Code == 200;
        }
        catch
        {
            return false;
        }
    }

    private string BuildCurrentWeatherUrl(string location)
    {
        ValidateApiKey();
        return $"{_apiConfig.Weather.BaseUrl}/weather?q={Uri.EscapeDataString(location)}&appid={_apiConfig.Weather.ApiKey}&units={_apiConfig.Weather.Units}";
    }

    private string BuildForecastUrl(string location)
    {
        ValidateApiKey();
        return $"{_apiConfig.Weather.BaseUrl}/forecast?q={Uri.EscapeDataString(location)}&appid={_apiConfig.Weather.ApiKey}&units={_apiConfig.Weather.Units}";
    }

    private string BuildCurrentWeatherUrlByCoordinates(double latitude, double longitude)
    {
        ValidateApiKey();
        return $"{_apiConfig.Weather.BaseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiConfig.Weather.ApiKey}&units={_apiConfig.Weather.Units}";
    }

    private string BuildForecastUrlByCoordinates(double latitude, double longitude)
    {
        ValidateApiKey();
        return $"{_apiConfig.Weather.BaseUrl}/forecast?lat={latitude}&lon={longitude}&appid={_apiConfig.Weather.ApiKey}&units={_apiConfig.Weather.Units}";
    }

    private void ValidateApiKey()
    {
        if (string.IsNullOrWhiteSpace(_apiConfig.Weather.ApiKey))
        {
            throw new InvalidOperationException("Weather API key is not configured. Please check your configuration.");
        }
    }
}
