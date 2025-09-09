namespace Global_Insights_Dashboard.Services.Interfaces;

/// <summary>
/// Service for caching API responses and application data
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Store data in cache with expiration
    /// </summary>
    /// <typeparam name="T">Type of data to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="data">Data to cache</param>
    /// <param name="expiration">Cache expiration time</param>
    Task SetAsync<T>(string key, T data, TimeSpan? expiration = null);

    /// <summary>
    /// Retrieve data from cache
    /// </summary>
    /// <typeparam name="T">Type of data to retrieve</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached data or default value</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Check if key exists in cache and is not expired
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if key exists and is valid</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Remove item from cache
    /// </summary>
    /// <param name="key">Cache key</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Clear all cached items
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Clear expired items from cache
    /// </summary>
    Task ClearExpiredAsync();

    /// <summary>
    /// Get cache statistics
    /// </summary>
    /// <returns>Cache statistics</returns>
    Task<CacheStatistics> GetStatisticsAsync();
}

/// <summary>
/// Cache statistics information
/// </summary>
public class CacheStatistics
{
    public int TotalItems { get; set; }
    public int ExpiredItems { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime LastCleanup { get; set; }
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    
    public double HitRatio => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0;
    public int TotalRequests => HitCount + MissCount;
}
