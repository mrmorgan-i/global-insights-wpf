using Newtonsoft.Json;

namespace Global_Insights_Dashboard.Models.DTOs;

/// <summary>
/// Alpha Vantage stock quote response
/// </summary>
public class StockQuoteResponse
{
    [JsonProperty("Global Quote")]
    public GlobalQuote? Quote { get; set; }

    [JsonProperty("Error Message")]
    public string? ErrorMessage { get; set; }

    [JsonProperty("Note")]
    public string? Note { get; set; }

    [JsonProperty("Information")]
    public string? Information { get; set; }

    public bool IsSuccess => Quote != null && string.IsNullOrEmpty(ErrorMessage) && string.IsNullOrEmpty(Information);
    public bool IsRateLimited => (!string.IsNullOrEmpty(Note) && Note.Contains("rate")) || 
                                (!string.IsNullOrEmpty(Information) && Information.Contains("rate limit"));
}

/// <summary>
/// Global quote data from Alpha Vantage
/// </summary>
public class GlobalQuote
{
    [JsonProperty("01. symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("02. open")]
    public string OpenString { get; set; } = string.Empty;

    [JsonProperty("03. high")]
    public string HighString { get; set; } = string.Empty;

    [JsonProperty("04. low")]
    public string LowString { get; set; } = string.Empty;

    [JsonProperty("05. price")]
    public string PriceString { get; set; } = string.Empty;

    [JsonProperty("06. volume")]
    public string VolumeString { get; set; } = string.Empty;

    [JsonProperty("07. latest trading day")]
    public string LatestTradingDay { get; set; } = string.Empty;

    [JsonProperty("08. previous close")]
    public string PreviousCloseString { get; set; } = string.Empty;

    [JsonProperty("09. change")]
    public string ChangeString { get; set; } = string.Empty;

    [JsonProperty("10. change percent")]
    public string ChangePercentString { get; set; } = string.Empty;

    // Computed properties with proper types
    public double Open => ParseDouble(OpenString);
    public double High => ParseDouble(HighString);
    public double Low => ParseDouble(LowString);
    public double Price => ParseDouble(PriceString);
    public long Volume => ParseLong(VolumeString);
    public double PreviousClose => ParseDouble(PreviousCloseString);
    public double Change => ParseDouble(ChangeString);
    public double ChangePercent => ParsePercent(ChangePercentString);

    // UI-friendly properties
    public string PriceDisplay => $"${Price:F2}";
    public string ChangeDisplay => $"{(Change >= 0 ? "+" : "")}{Change:F2}";
    public string ChangePercentDisplay => $"({(ChangePercent >= 0 ? "+" : "")}{ChangePercent:F2}%)";
    public bool IsPositive => Change >= 0;
    public bool IsNegative => Change < 0;
    public string TrendIcon => Change >= 0 ? "📈" : "📉";
    public DateTime TradingDate => DateTime.TryParse(LatestTradingDay, out var date) ? date : DateTime.MinValue;

    private static double ParseDouble(string value)
    {
        return double.TryParse(value, out var result) ? result : 0.0;
    }

    private static long ParseLong(string value)
    {
        return long.TryParse(value, out var result) ? result : 0L;
    }

    private static double ParsePercent(string value)
    {
        var cleanValue = value.Replace("%", "").Trim();
        return double.TryParse(cleanValue, out var result) ? result : 0.0;
    }
}

/// <summary>
/// Alpha Vantage company search response
/// </summary>
public class CompanySearchResponse
{
    [JsonProperty("bestMatches")]
    public List<CompanyMatch> BestMatches { get; set; } = new();

    [JsonProperty("Error Message")]
    public string? ErrorMessage { get; set; }

    [JsonProperty("Note")]
    public string? Note { get; set; }

    [JsonProperty("Information")]
    public string? Information { get; set; }

    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage) && string.IsNullOrEmpty(Information) && BestMatches.Any();
    public bool IsRateLimited => (!string.IsNullOrEmpty(Note) && Note.Contains("rate")) || 
                                (!string.IsNullOrEmpty(Information) && Information.Contains("rate limit"));
}

/// <summary>
/// Company match result from search
/// </summary>
public class CompanyMatch
{
    [JsonProperty("1. symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("2. name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("3. type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("4. region")]
    public string Region { get; set; } = string.Empty;

    [JsonProperty("5. marketOpen")]
    public string MarketOpen { get; set; } = string.Empty;

    [JsonProperty("6. marketClose")]
    public string MarketClose { get; set; } = string.Empty;

    [JsonProperty("7. timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonProperty("8. currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("9. matchScore")]
    public string MatchScoreString { get; set; } = string.Empty;

    public double MatchScore => double.TryParse(MatchScoreString, out var score) ? score : 0.0;
    public string DisplayName => $"{Symbol} - {Name}";
    public string RegionCurrency => $"{Region} ({Currency})";
}

/// <summary>
/// Alpha Vantage intraday time series response
/// </summary>
public class IntradayTimeSeriesResponse
{
    [JsonProperty("Meta Data")]
    public TimeSeriesMetaData? MetaData { get; set; }

    [JsonProperty("Time Series (5min)")]
    public Dictionary<string, TimeSeriesData>? TimeSeries5Min { get; set; }

    [JsonProperty("Time Series (15min)")]
    public Dictionary<string, TimeSeriesData>? TimeSeries15Min { get; set; }

    [JsonProperty("Time Series (30min)")]
    public Dictionary<string, TimeSeriesData>? TimeSeries30Min { get; set; }

    [JsonProperty("Time Series (60min)")]
    public Dictionary<string, TimeSeriesData>? TimeSeries60Min { get; set; }

    [JsonProperty("Error Message")]
    public string? ErrorMessage { get; set; }

    [JsonProperty("Note")]
    public string? Note { get; set; }

    [JsonProperty("Information")]
    public string? Information { get; set; }

    public bool IsSuccess => MetaData != null && string.IsNullOrEmpty(ErrorMessage) && string.IsNullOrEmpty(Information);
    public bool IsRateLimited => (!string.IsNullOrEmpty(Note) && Note.Contains("rate")) || 
                                (!string.IsNullOrEmpty(Information) && Information.Contains("rate limit"));

    public List<ChartDataPoint> GetChartData(string interval = "5min")
    {
        var timeSeries = interval switch
        {
            "15min" => TimeSeries15Min,
            "30min" => TimeSeries30Min,
            "60min" => TimeSeries60Min,
            _ => TimeSeries5Min
        };

        if (timeSeries == null) return new List<ChartDataPoint>();

        return timeSeries
            .Select(kvp => new ChartDataPoint
            {
                DateTime = DateTime.Parse(kvp.Key),
                Open = kvp.Value.Open,
                High = kvp.Value.High,
                Low = kvp.Value.Low,
                Close = kvp.Value.Close,
                Volume = kvp.Value.Volume
            })
            .OrderBy(x => x.DateTime)
            .ToList();
    }
}

/// <summary>
/// Time series metadata
/// </summary>
public class TimeSeriesMetaData
{
    [JsonProperty("1. Information")]
    public string Information { get; set; } = string.Empty;

    [JsonProperty("2. Symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("3. Last Refreshed")]
    public string LastRefreshed { get; set; } = string.Empty;

    [JsonProperty("4. Interval")]
    public string Interval { get; set; } = string.Empty;

    [JsonProperty("5. Output Size")]
    public string OutputSize { get; set; } = string.Empty;

    [JsonProperty("6. Time Zone")]
    public string TimeZone { get; set; } = string.Empty;

    public DateTime LastRefreshedDateTime => DateTime.TryParse(LastRefreshed, out var date) ? date : DateTime.MinValue;
}

/// <summary>
/// Individual time series data point
/// </summary>
public class TimeSeriesData
{
    [JsonProperty("1. open")]
    public string OpenString { get; set; } = string.Empty;

    [JsonProperty("2. high")]
    public string HighString { get; set; } = string.Empty;

    [JsonProperty("3. low")]
    public string LowString { get; set; } = string.Empty;

    [JsonProperty("4. close")]
    public string CloseString { get; set; } = string.Empty;

    [JsonProperty("5. volume")]
    public string VolumeString { get; set; } = string.Empty;

    public double Open => double.TryParse(OpenString, out var result) ? result : 0.0;
    public double High => double.TryParse(HighString, out var result) ? result : 0.0;
    public double Low => double.TryParse(LowString, out var result) ? result : 0.0;
    public double Close => double.TryParse(CloseString, out var result) ? result : 0.0;
    public long Volume => long.TryParse(VolumeString, out var result) ? result : 0L;
}

/// <summary>
/// Chart data point for visualization
/// </summary>
public class ChartDataPoint
{
    public DateTime DateTime { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public long Volume { get; set; }

    public string TimeLabel => DateTime.ToString("HH:mm");
    public string DateLabel => DateTime.ToString("MMM dd");
    public double Change => Close - Open;
    public bool IsPositive => Change >= 0;
}

/// <summary>
/// Popular stock symbols for quick access
/// </summary>
public class PopularStock
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;

    public static List<PopularStock> GetPopularStocks()
    {
        return new List<PopularStock>
        {
            new() { Symbol = "AAPL", CompanyName = "Apple Inc.", Sector = "Technology", Exchange = "NASDAQ" },
            new() { Symbol = "MSFT", CompanyName = "Microsoft Corporation", Sector = "Technology", Exchange = "NASDAQ" },
            new() { Symbol = "GOOGL", CompanyName = "Alphabet Inc.", Sector = "Technology", Exchange = "NASDAQ" },
            new() { Symbol = "AMZN", CompanyName = "Amazon.com Inc.", Sector = "Consumer Discretionary", Exchange = "NASDAQ" },
            new() { Symbol = "TSLA", CompanyName = "Tesla Inc.", Sector = "Consumer Discretionary", Exchange = "NASDAQ" },
            new() { Symbol = "META", CompanyName = "Meta Platforms Inc.", Sector = "Technology", Exchange = "NASDAQ" },
            new() { Symbol = "NVDA", CompanyName = "NVIDIA Corporation", Sector = "Technology", Exchange = "NASDAQ" },
            new() { Symbol = "NFLX", CompanyName = "Netflix Inc.", Sector = "Communication Services", Exchange = "NASDAQ" },
            new() { Symbol = "AMD", CompanyName = "Advanced Micro Devices", Sector = "Technology", Exchange = "NASDAQ" },
            new() { Symbol = "PYPL", CompanyName = "PayPal Holdings Inc.", Sector = "Financial Services", Exchange = "NASDAQ" }
        };
    }
}

/// <summary>
/// Stock watchlist item
/// </summary>
public class WatchlistItem
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateTime DateAdded { get; set; } = DateTime.Now;
    public double? LastPrice { get; set; }
    public double? LastChange { get; set; }
    public DateTime? LastUpdated { get; set; }

    public string DisplayName => string.IsNullOrEmpty(CompanyName) ? Symbol : $"{Symbol} - {CompanyName}";
    public string LastPriceDisplay => LastPrice?.ToString("C2") ?? "N/A";
    public string LastChangeDisplay => LastChange?.ToString("+0.00;-0.00;0.00") ?? "N/A";
}
