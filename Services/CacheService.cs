using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Global_Insights_Dashboard.Models.Configuration;
using Global_Insights_Dashboard.Services.Interfaces;

namespace Global_Insights_Dashboard.Services;

/// <summary>
/// Implementation of cache service using in-memory and file-based storage
/// </summary>
public class CacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheItem> _memoryCache;
    private readonly AppSettings _appSettings;
    private readonly string _cacheDirectory;
    private readonly SemaphoreSlim _cleanupSemaphore;
    private readonly CacheStatistics _statistics;
    private DateTime _lastCleanup = DateTime.MinValue;

    public CacheService(IOptions<AppSettings> appSettings, IConfigurationService configurationService)
    {
        _memoryCache = new ConcurrentDictionary<string, CacheItem>();
        _appSettings = appSettings.Value;
        _cacheDirectory = Path.Combine(configurationService.GetConfigurationDirectory(), "cache");
        _cleanupSemaphore = new SemaphoreSlim(1, 1);
        _statistics = new CacheStatistics();

        // Ensure cache directory exists
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }

        // Start background cleanup task
        _ = StartBackgroundCleanup();
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        var defaultExpiration = TimeSpan.FromMinutes(_appSettings.Cache.CacheExpirationMinutes);
        var actualExpiration = expiration ?? defaultExpiration;
        var expiryTime = DateTime.UtcNow.Add(actualExpiration);

        var cacheItem = new CacheItem
        {
            Key = key,
            Data = data,
            ExpiryTime = expiryTime,
            CreatedTime = DateTime.UtcNow
        };

        // Store in memory cache
        _memoryCache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);

        // Store in file cache for persistence
        if (_appSettings.Cache.EnableOfflineMode)
        {
            await StoreToDiskAsync<T>(key, cacheItem);
        }

        // Cleanup if cache is too large
        await CleanupIfNeeded();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return default;

        // Try memory cache first
        if (_memoryCache.TryGetValue(key, out var memoryItem))
        {
            if (memoryItem.ExpiryTime > DateTime.UtcNow)
            {
                _statistics.HitCount++;
                return DeserializeCacheData<T>(memoryItem.Data);
            }
            else
            {
                // Item expired, remove it
                _memoryCache.TryRemove(key, out _);
            }
        }

        // Try disk cache if enabled
        if (_appSettings.Cache.EnableOfflineMode)
        {
            var diskItem = await LoadFromDiskAsync<T>(key);
            if (diskItem != null && diskItem.ExpiryTime > DateTime.UtcNow)
            {
                // Restore to memory cache
                _memoryCache.TryAdd(key, diskItem);
                _statistics.HitCount++;
                return DeserializeCacheData<T>(diskItem.Data);
            }
        }

        _statistics.MissCount++;
        return default;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        // Check memory cache
        if (_memoryCache.TryGetValue(key, out var item))
        {
            if (item.ExpiryTime > DateTime.UtcNow)
                return true;
            
            // Remove expired item
            _memoryCache.TryRemove(key, out _);
        }

        // Check disk cache if enabled
        if (_appSettings.Cache.EnableOfflineMode)
        {
            var diskItem = await LoadFromDiskAsync<object>(key);
            return diskItem != null && diskItem.ExpiryTime > DateTime.UtcNow;
        }

        return false;
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        // Remove from memory
        _memoryCache.TryRemove(key, out _);

        // Remove from disk
        var filePath = GetCacheFilePath(key);
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // Ignore file deletion errors
            }
        }

        await Task.CompletedTask;
    }

    public async Task ClearAsync()
    {
        // Clear memory cache
        _memoryCache.Clear();

        // Clear disk cache
        if (Directory.Exists(_cacheDirectory))
        {
            try
            {
                var files = Directory.GetFiles(_cacheDirectory, "*.cache");
                await Task.Run(() =>
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // Ignore individual file deletion errors
                        }
                    }
                });
            }
            catch
            {
                // Ignore directory errors
            }
        }

        // Reset statistics
        _statistics.HitCount = 0;
        _statistics.MissCount = 0;
        _statistics.LastCleanup = DateTime.UtcNow;
    }

    public async Task ClearExpiredAsync()
    {
        await _cleanupSemaphore.WaitAsync();
        
        try
        {
            var now = DateTime.UtcNow;
            var expiredKeys = new List<string>();

            // Find expired items in memory
            foreach (var kvp in _memoryCache)
            {
                if (kvp.Value.ExpiryTime <= now)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            // Remove expired items from memory
            foreach (var key in expiredKeys)
            {
                _memoryCache.TryRemove(key, out _);
            }

            // Clean up disk cache
            if (Directory.Exists(_cacheDirectory))
            {
                await Task.Run(() =>
                {
                    var files = Directory.GetFiles(_cacheDirectory, "*.cache");
                    foreach (var file in files)
                    {
                        try
                        {
                            var item = LoadCacheItemFromFile(file);
                            if (item?.ExpiryTime <= now)
                            {
                                File.Delete(file);
                            }
                        }
                        catch
                        {
                            // Ignore errors and continue
                        }
                    }
                });
            }

            _statistics.LastCleanup = DateTime.UtcNow;
            _lastCleanup = DateTime.UtcNow;
        }
        finally
        {
            _cleanupSemaphore.Release();
        }
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        _statistics.TotalItems = _memoryCache.Count;
        _statistics.ExpiredItems = _memoryCache.Values.Count(i => i.ExpiryTime <= DateTime.UtcNow);
        _statistics.LastCleanup = _lastCleanup;

        if (Directory.Exists(_cacheDirectory))
        {
            await Task.Run(() =>
            {
                var files = Directory.GetFiles(_cacheDirectory, "*.cache");
                _statistics.TotalSizeBytes = files.Sum(f =>
                {
                    try
                    {
                        return new FileInfo(f).Length;
                    }
                    catch
                    {
                        return 0;
                    }
                });
            });
        }

        return _statistics;
    }

    private async Task StoreToDiskAsync<T>(string key, CacheItem item)
    {
        try
        {
            var filePath = GetCacheFilePath(key);
            var json = JsonConvert.SerializeObject(item, Formatting.None);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch
        {
            // Ignore disk storage errors - memory cache still works
        }
    }

    private async Task<CacheItem?> LoadFromDiskAsync<T>(string key)
    {
        try
        {
            var filePath = GetCacheFilePath(key);
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<CacheItem>(json);
        }
        catch
        {
            return null;
        }
    }

    private CacheItem? LoadCacheItemFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<CacheItem>(json);
        }
        catch
        {
            return null;
        }
    }

    private string GetCacheFilePath(string key)
    {
        var safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cacheDirectory, $"{safeKey}.cache");
    }

    private async Task CleanupIfNeeded()
    {
        if (_memoryCache.Count > _appSettings.Cache.MaxCacheSize)
        {
            await ClearExpiredAsync();
        }

        // Periodic cleanup
        if (DateTime.UtcNow - _lastCleanup > TimeSpan.FromHours(1))
        {
            _ = Task.Run(ClearExpiredAsync);
        }
    }

    private async Task StartBackgroundCleanup()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1));
                await ClearExpiredAsync();
            }
            catch
            {
                // Continue background cleanup even if errors occur
            }
        }
    }

    /// <summary>
    /// Helper method to properly deserialize cached data from JObject to target type
    /// </summary>
    private static T? DeserializeCacheData<T>(object? data)
    {
        if (data == null)
            return default;

        // If data is already the target type, return it directly
        if (data is T directData)
            return directData;

        // If data is a JObject (from JSON deserialization), convert it to target type
        if (data is JObject jObject)
        {
            return jObject.ToObject<T>();
        }

        // Try to serialize and deserialize as fallback
        try
        {
            var json = JsonConvert.SerializeObject(data);
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch
        {
            return default;
        }
    }
}

/// <summary>
/// Internal cache item structure
/// </summary>
internal class CacheItem
{
    public string Key { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime ExpiryTime { get; set; }
    public DateTime CreatedTime { get; set; }
}
