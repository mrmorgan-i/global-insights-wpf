using Global_Insights_Dashboard.Models.DTOs;

namespace Global_Insights_Dashboard.Services.Interfaces;

/// <summary>
/// Service for news data retrieval from NewsAPI
/// </summary>
public interface INewsService
{
    /// <summary>
    /// Get top headlines by country and category
    /// </summary>
    /// <param name="country">Country code</param>
    /// <param name="category">News category</param>
    /// <param name="pageSize">Number of articles to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>News response with articles</returns>
    Task<NewsResponse?> GetTopHeadlinesAsync(string country = "us", string category = "general", int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for news articles by keyword
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="pageSize">Number of articles to retrieve</param>
    /// <param name="page">Page number</param>
    /// <param name="sortBy">Sort articles by (publishedAt, relevancy, popularity)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>News response with articles</returns>
    Task<NewsResponse?> SearchNewsAsync(string query, int pageSize = 20, int page = 1, string sortBy = "publishedAt", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get headlines using a search request object
    /// </summary>
    /// <param name="request">News search request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>News response with articles</returns>
    Task<NewsResponse?> GetHeadlinesAsync(NewsSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search everything (all articles) with advanced parameters
    /// </summary>
    /// <param name="request">News search request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>News response with articles</returns>
    Task<NewsResponse?> SearchEverythingAsync(NewsSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test API connectivity and key validity
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if API is accessible</returns>
    Task<bool> TestApiConnectivityAsync(CancellationToken cancellationToken = default);
}
