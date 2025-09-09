using Global_Insights_Dashboard.Models.DTOs;

namespace Global_Insights_Dashboard.Services.Interfaces;

/// <summary>
/// Service for financial data retrieval from Alpha Vantage API
/// </summary>
public interface IFinanceService
{
    /// <summary>
    /// Get real-time stock quote for a symbol
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., AAPL, MSFT)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stock quote data</returns>
    Task<StockQuoteResponse?> GetStockQuoteAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for companies by keyword
    /// </summary>
    /// <param name="keywords">Search keywords</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Company search results</returns>
    Task<CompanySearchResponse?> SearchCompaniesAsync(string keywords, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get intraday time series data for charting
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <param name="interval">Time interval (1min, 5min, 15min, 30min, 60min)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Intraday time series data</returns>
    Task<IntradayTimeSeriesResponse?> GetIntradayDataAsync(string symbol, string interval = "5min", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get quotes for multiple popular stocks
    /// </summary>
    /// <param name="symbols">List of stock symbols</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of symbol to quote data</returns>
    Task<Dictionary<string, StockQuoteResponse?>> GetMultipleQuotesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test API connectivity and key validity
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if API is accessible</returns>
    Task<bool> TestApiConnectivityAsync(CancellationToken cancellationToken = default);
}
