using System.Net.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Global_Insights_Dashboard.Models.Configuration;
using Global_Insights_Dashboard.Models.DTOs;
using Global_Insights_Dashboard.Services.Interfaces;

namespace Global_Insights_Dashboard.Services;

/// <summary>
/// Implementation of news service using NewsAPI
/// </summary>
public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguration _apiConfig;

    public NewsService(HttpClient httpClient, IOptions<ApiConfiguration> apiConfig)
    {
        _httpClient = httpClient;
        _apiConfig = apiConfig.Value;
        
        // Set API key header for NewsAPI
        if (!string.IsNullOrWhiteSpace(_apiConfig.News.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiConfig.News.ApiKey);
        }
    }

    public async Task<NewsResponse?> GetTopHeadlinesAsync(string country = "us", string category = "general", int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var url = BuildTopHeadlinesUrl(country, category, pageSize);

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var newsData = JsonConvert.DeserializeObject<NewsResponse>(response);
            
            return newsData;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve top headlines for {country}/{category}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException($"News API request timed out for {country}/{category}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse news data for {country}/{category}: {ex.Message}", ex);
        }
    }

    public async Task<NewsResponse?> SearchNewsAsync(string query, int pageSize = 20, int page = 1, string sortBy = "publishedAt", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty", nameof(query));

        var url = BuildSearchUrl(query, pageSize, page, sortBy);

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var newsData = JsonConvert.DeserializeObject<NewsResponse>(response);
            
            return newsData;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to search news for query '{query}': {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException($"News search timed out for query '{query}'", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse news search results for query '{query}': {ex.Message}", ex);
        }
    }

    public async Task<NewsResponse?> GetHeadlinesAsync(NewsSearchRequest request, CancellationToken cancellationToken = default)
    {
        var url = BuildHeadlinesUrl(request);

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var newsData = JsonConvert.DeserializeObject<NewsResponse>(response);
            
            return newsData;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve headlines: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException("Headlines request timed out", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse headlines data: {ex.Message}", ex);
        }
    }

    public async Task<NewsResponse?> SearchEverythingAsync(NewsSearchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("Query cannot be empty for everything search", nameof(request));

        var url = BuildEverythingUrl(request);

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var newsData = JsonConvert.DeserializeObject<NewsResponse>(response);
            
            return newsData;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to search everything: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException("Everything search timed out", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse everything search results: {ex.Message}", ex);
        }
    }

    public async Task<bool> TestApiConnectivityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Test with a simple query
            var result = await GetTopHeadlinesAsync("us", "general", 1, cancellationToken);
            return result != null && result.IsSuccess;
        }
        catch
        {
            return false;
        }
    }

    private string BuildTopHeadlinesUrl(string country, string category, int pageSize)
    {
        ValidateApiKey();
        var parameters = new List<string>
        {
            $"country={Uri.EscapeDataString(country)}",
            $"category={Uri.EscapeDataString(category)}",
            $"pageSize={Math.Min(pageSize, 100)}" // NewsAPI limit is 100
        };

        return $"{_apiConfig.News.BaseUrl}/top-headlines?{string.Join("&", parameters)}";
    }

    private string BuildSearchUrl(string query, int pageSize, int page, string sortBy)
    {
        ValidateApiKey();
        var parameters = new List<string>
        {
            $"q={Uri.EscapeDataString(query)}",
            $"pageSize={Math.Min(pageSize, 100)}",
            $"page={Math.Max(page, 1)}",
            $"sortBy={Uri.EscapeDataString(sortBy)}",
            "language=en"
        };

        return $"{_apiConfig.News.BaseUrl}/everything?{string.Join("&", parameters)}";
    }

    private string BuildHeadlinesUrl(NewsSearchRequest request)
    {
        ValidateApiKey();
        var parameters = new List<string>();

        if (request.Country.HasValue)
        {
            parameters.Add($"country={request.GetCountryCode()}");
        }

        if (request.Category.HasValue)
        {
            parameters.Add($"category={request.GetCategoryName()}");
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            parameters.Add($"q={Uri.EscapeDataString(request.Query)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Sources))
        {
            parameters.Add($"sources={Uri.EscapeDataString(request.Sources)}");
        }

        parameters.Add($"pageSize={Math.Min(request.PageSize, 100)}");
        parameters.Add($"page={Math.Max(request.Page, 1)}");

        return $"{_apiConfig.News.BaseUrl}/top-headlines?{string.Join("&", parameters)}";
    }

    private string BuildEverythingUrl(NewsSearchRequest request)
    {
        ValidateApiKey();
        var parameters = new List<string>
        {
            $"q={Uri.EscapeDataString(request.Query!)}",
            $"pageSize={Math.Min(request.PageSize, 100)}",
            $"page={Math.Max(request.Page, 1)}"
        };

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            parameters.Add($"language={Uri.EscapeDataString(request.Language)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Sources))
        {
            parameters.Add($"sources={Uri.EscapeDataString(request.Sources)}");
        }

        if (request.From.HasValue)
        {
            parameters.Add($"from={request.From.Value:yyyy-MM-ddTHH:mm:ss}");
        }

        if (request.To.HasValue)
        {
            parameters.Add($"to={request.To.Value:yyyy-MM-ddTHH:mm:ss}");
        }

        parameters.Add("sortBy=publishedAt");

        return $"{_apiConfig.News.BaseUrl}/everything?{string.Join("&", parameters)}";
    }

    private void ValidateApiKey()
    {
        if (string.IsNullOrWhiteSpace(_apiConfig.News.ApiKey))
        {
            throw new InvalidOperationException("News API key is not configured. Please check your configuration.");
        }
    }
}
