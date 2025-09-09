using System.Net.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Global_Insights_Dashboard.Models.Configuration;
using Global_Insights_Dashboard.Models.DTOs;
using Global_Insights_Dashboard.Services.Interfaces;

namespace Global_Insights_Dashboard.Services;

/// <summary>
/// Implementation of finance service using Polygon.io API
/// </summary>
public class FinanceService : IFinanceService
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguration _apiConfig;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private DateTime _lastRequest = DateTime.MinValue;

    public FinanceService(HttpClient httpClient, IOptions<ApiConfiguration> apiConfig)
    {
        _httpClient = httpClient;
        _apiConfig = apiConfig.Value;
        _rateLimitSemaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<StockQuoteResponse?> GetStockQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        await WaitForRateLimit(cancellationToken);

        try
        {
            // Get previous day price data (required)
            var priceUrl = BuildPreviousDayUrl(symbol);
            var priceResponse = await _httpClient.GetStringAsync(priceUrl, cancellationToken);
            var priceData = JsonConvert.DeserializeObject<PolygonPreviousDayResponse>(priceResponse);

            if (priceData?.Status != "OK" || priceData.PrimaryBar == null)
            {
                throw new InvalidOperationException($"No price data found for symbol {symbol}");
            }

            // Get company details (optional - enhance with company info)
            PolygonTickerDetailsResponse? detailsData = null;
            try
            {
                await WaitForRateLimit(cancellationToken); // Respect rate limits for second call
                var detailsUrl = BuildTickerDetailsUrl(symbol);
                var detailsResponse = await _httpClient.GetStringAsync(detailsUrl, cancellationToken);
                detailsData = JsonConvert.DeserializeObject<PolygonTickerDetailsResponse>(detailsResponse);
            }
            catch
            {
                // If details fail, continue with just price data
                // Details are nice-to-have but not essential
            }

            // Convert Polygon.io data to our existing StockQuoteResponse format
            var polygonQuote = PolygonStockQuote.FromPolygonData(priceData, detailsData);
            return ConvertToStockQuoteResponse(polygonQuote);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve stock quote for {symbol}: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new TimeoutException($"Stock quote request timed out for {symbol}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse stock quote data for {symbol}: {ex.Message}", ex);
        }
    }

    public async Task<CompanySearchResponse?> SearchCompaniesAsync(string keywords, CancellationToken cancellationToken = default)
    {
        // TODO: Implement Polygon.io company search
        // For now, return empty search results to maintain compatibility
        await Task.Delay(1, cancellationToken); // Simulate async operation
        
        return new CompanySearchResponse
        {
            BestMatches = new List<CompanyMatch>()
        };
    }

    public async Task<IntradayTimeSeriesResponse?> GetIntradayDataAsync(string symbol, string interval = "5min", CancellationToken cancellationToken = default)
    {
        // TODO: Implement Polygon.io intraday data
        // For now, return empty time series to maintain compatibility
        await Task.Delay(1, cancellationToken); // Simulate async operation
        
        return new IntradayTimeSeriesResponse
        {
            TimeSeries5Min = new Dictionary<string, TimeSeriesData>()
        };
    }

    public async Task<Dictionary<string, StockQuoteResponse?>> GetMultipleQuotesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, StockQuoteResponse?>();
        
        foreach (var symbol in symbols)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var quote = await GetStockQuoteAsync(symbol, cancellationToken);
                results[symbol] = quote;
            }
            catch (Exception ex)
            {
                // Log error but continue with other symbols
                System.Diagnostics.Debug.WriteLine($"Failed to get quote for {symbol}: {ex.Message}");
                results[symbol] = null;
            }
        }

        return results;
    }

    public async Task<bool> TestApiConnectivityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Test with a known good symbol
            var result = await GetStockQuoteAsync("AAPL", cancellationToken);
            return result != null && result.IsSuccess;
        }
        catch
        {
            return false;
        }
    }

    private async Task WaitForRateLimit(CancellationToken cancellationToken = default)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);
        
        try
        {
            var timeSinceLastRequest = DateTime.Now - _lastRequest;
            var minInterval = TimeSpan.FromMilliseconds(_apiConfig.Finance.RequestDelayMs);
            
            if (timeSinceLastRequest < minInterval)
            {
                var delay = minInterval - timeSinceLastRequest;
                await Task.Delay(delay, cancellationToken);
            }
            
            _lastRequest = DateTime.Now;
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    private string BuildPreviousDayUrl(string symbol)
    {
        ValidateApiKey();
        return $"{_apiConfig.Finance.BaseUrl}/aggs/ticker/{Uri.EscapeDataString(symbol)}/prev?adjusted=true&apikey={_apiConfig.Finance.ApiKey}";
    }

    private string BuildTickerDetailsUrl(string symbol)
    {
        ValidateApiKey();
        return $"https://api.polygon.io/v3/reference/tickers/{Uri.EscapeDataString(symbol)}?apikey={_apiConfig.Finance.ApiKey}";
    }

    private string BuildSearchUrl(string keywords)
    {
        ValidateApiKey();
        // Polygon.io search endpoint - searches for tickers matching keywords
        return $"https://api.polygon.io/v3/reference/tickers?search={Uri.EscapeDataString(keywords)}&active=true&limit=10&apikey={_apiConfig.Finance.ApiKey}";
    }

    private string BuildIntradayUrl(string symbol, string interval)
    {
        ValidateApiKey();
        // Polygon.io aggregates endpoint for intraday data (last 2 days)
        var timespan = interval switch
        {
            "1min" => "minute",
            "5min" => "minute", 
            "15min" => "minute",
            "30min" => "minute",
            "60min" => "hour",
            _ => "minute"
        };
        
        var multiplier = interval switch
        {
            "1min" => "1",
            "5min" => "5",
            "15min" => "15", 
            "30min" => "30",
            "60min" => "1",
            _ => "5"
        };

        var from = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
        var to = DateTime.Now.ToString("yyyy-MM-dd");
        
        return $"{_apiConfig.Finance.BaseUrl}/aggs/ticker/{Uri.EscapeDataString(symbol)}/range/{multiplier}/{timespan}/{from}/{to}?adjusted=true&sort=asc&apikey={_apiConfig.Finance.ApiKey}";
    }

    private void ValidateApiKey()
    {
        if (string.IsNullOrWhiteSpace(_apiConfig.Finance.ApiKey))
        {
            throw new InvalidOperationException("Polygon.io API key is not configured. Please check your configuration.");
        }
    }

    /// <summary>
    /// Convert PolygonStockQuote to existing StockQuoteResponse format for UI compatibility
    /// </summary>
    private static StockQuoteResponse ConvertToStockQuoteResponse(PolygonStockQuote polygonQuote)
    {
        return new StockQuoteResponse
        {
            Quote = new GlobalQuote
            {
                Symbol = polygonQuote.Symbol,
                OpenString = polygonQuote.Open.ToString("F2"),
                HighString = polygonQuote.High.ToString("F2"),
                LowString = polygonQuote.Low.ToString("F2"),
                PriceString = polygonQuote.Price.ToString("F2"),
                VolumeString = polygonQuote.Volume.ToString("F0"),
                LatestTradingDay = polygonQuote.LastUpdated.ToString("yyyy-MM-dd"),
                PreviousCloseString = polygonQuote.Open.ToString("F2"), // Using Open as previous close approximation
                ChangeString = polygonQuote.Change.ToString("F2"),
                ChangePercentString = $"{polygonQuote.ChangePercent:F2}%"
            }
        };
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _rateLimitSemaphore?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
