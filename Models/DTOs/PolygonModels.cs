using Newtonsoft.Json;

namespace Global_Insights_Dashboard.Models.DTOs;

/// <summary>
/// Response from Polygon.io Previous Day Aggregate endpoint
/// </summary>
public class PolygonPreviousDayResponse
{
    [JsonProperty("ticker")]
    public string Ticker { get; set; } = string.Empty;

    [JsonProperty("queryCount")]
    public int QueryCount { get; set; }

    [JsonProperty("resultsCount")]
    public int ResultsCount { get; set; }

    [JsonProperty("adjusted")]
    public bool Adjusted { get; set; }

    [JsonProperty("results")]
    public List<PolygonAggregateBar> Results { get; set; } = new();

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("request_id")]
    public string RequestId { get; set; } = string.Empty;

    [JsonProperty("count")]
    public int Count { get; set; }

    /// <summary>
    /// Indicates if the API call was successful
    /// </summary>
    public bool IsSuccess => Status == "OK" && Results.Any();

    /// <summary>
    /// Gets the first (and typically only) result bar
    /// </summary>
    public PolygonAggregateBar? PrimaryBar => Results.FirstOrDefault();
}

/// <summary>
/// Individual aggregate bar data from Polygon.io
/// </summary>
public class PolygonAggregateBar
{
    [JsonProperty("T")]
    public string Ticker { get; set; } = string.Empty;

    [JsonProperty("v")]
    public double Volume { get; set; }

    [JsonProperty("vw")]
    public double VolumeWeightedAveragePrice { get; set; }

    [JsonProperty("o")]
    public double Open { get; set; }

    [JsonProperty("c")]
    public double Close { get; set; }

    [JsonProperty("h")]
    public double High { get; set; }

    [JsonProperty("l")]
    public double Low { get; set; }

    [JsonProperty("t")]
    public long Timestamp { get; set; }

    [JsonProperty("n")]
    public int NumberOfTransactions { get; set; }

    /// <summary>
    /// Convert timestamp to DateTime
    /// </summary>
    public DateTime Date => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;

    /// <summary>
    /// Calculate daily change
    /// </summary>
    public double Change => Close - Open;

    /// <summary>
    /// Calculate daily change percentage
    /// </summary>
    public double ChangePercent => Open != 0 ? (Change / Open) * 100 : 0;
}

/// <summary>
/// Response from Polygon.io Ticker Details endpoint
/// </summary>
public class PolygonTickerDetailsResponse
{
    [JsonProperty("request_id")]
    public string RequestId { get; set; } = string.Empty;

    [JsonProperty("results")]
    public PolygonTickerDetails? Results { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the API call was successful
    /// </summary>
    public bool IsSuccess => Status == "OK" && Results != null;
}

/// <summary>
/// Detailed information about a ticker from Polygon.io
/// </summary>
public class PolygonTickerDetails
{
    [JsonProperty("ticker")]
    public string Ticker { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("market")]
    public string Market { get; set; } = string.Empty;

    [JsonProperty("locale")]
    public string Locale { get; set; } = string.Empty;

    [JsonProperty("primary_exchange")]
    public string PrimaryExchange { get; set; } = string.Empty;

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("active")]
    public bool Active { get; set; }

    [JsonProperty("currency_name")]
    public string CurrencyName { get; set; } = string.Empty;

    [JsonProperty("cik")]
    public string Cik { get; set; } = string.Empty;

    [JsonProperty("composite_figi")]
    public string CompositeFigi { get; set; } = string.Empty;

    [JsonProperty("share_class_figi")]
    public string ShareClassFigi { get; set; } = string.Empty;

    [JsonProperty("market_cap")]
    public double MarketCap { get; set; }

    [JsonProperty("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonProperty("address")]
    public PolygonAddress? Address { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("sic_code")]
    public string SicCode { get; set; } = string.Empty;

    [JsonProperty("sic_description")]
    public string SicDescription { get; set; } = string.Empty;

    [JsonProperty("ticker_root")]
    public string TickerRoot { get; set; } = string.Empty;

    [JsonProperty("homepage_url")]
    public string HomepageUrl { get; set; } = string.Empty;

    [JsonProperty("total_employees")]
    public int TotalEmployees { get; set; }

    [JsonProperty("list_date")]
    public string ListDate { get; set; } = string.Empty;

    [JsonProperty("branding")]
    public PolygonBranding? Branding { get; set; }

    [JsonProperty("share_class_shares_outstanding")]
    public long ShareClassSharesOutstanding { get; set; }

    [JsonProperty("weighted_shares_outstanding")]
    public long WeightedSharesOutstanding { get; set; }

    [JsonProperty("round_lot")]
    public int RoundLot { get; set; }

    /// <summary>
    /// Format market cap as a readable string
    /// </summary>
    public string FormattedMarketCap
    {
        get
        {
            if (MarketCap >= 1_000_000_000_000)
                return $"${MarketCap / 1_000_000_000_000:F2}T";
            if (MarketCap >= 1_000_000_000)
                return $"${MarketCap / 1_000_000_000:F2}B";
            if (MarketCap >= 1_000_000)
                return $"${MarketCap / 1_000_000:F2}M";
            return $"${MarketCap:F2}";
        }
    }
}

/// <summary>
/// Address information from Polygon.io
/// </summary>
public class PolygonAddress
{
    [JsonProperty("address1")]
    public string Address1 { get; set; } = string.Empty;

    [JsonProperty("city")]
    public string City { get; set; } = string.Empty;

    [JsonProperty("state")]
    public string State { get; set; } = string.Empty;

    [JsonProperty("postal_code")]
    public string PostalCode { get; set; } = string.Empty;

    public override string ToString()
    {
        var parts = new[] { Address1, City, State, PostalCode }.Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }
}

/// <summary>
/// Branding information from Polygon.io
/// </summary>
public class PolygonBranding
{
    [JsonProperty("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;

    [JsonProperty("icon_url")]
    public string IconUrl { get; set; } = string.Empty;
}

/// <summary>
/// Combined stock quote information using Polygon.io data
/// This class adapts Polygon.io responses to match our existing UI expectations
/// </summary>
public class PolygonStockQuote
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public double Price { get; set; }
    public double Change { get; set; }
    public double ChangePercent { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Volume { get; set; }
    public string MarketCap { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Create a PolygonStockQuote from Polygon.io API responses
    /// </summary>
    public static PolygonStockQuote FromPolygonData(PolygonPreviousDayResponse priceData, PolygonTickerDetailsResponse? detailsData = null)
    {
        var bar = priceData.PrimaryBar;
        var details = detailsData?.Results;

        if (bar == null)
            throw new ArgumentException("Price data is required", nameof(priceData));

        return new PolygonStockQuote
        {
            Symbol = bar.Ticker,
            CompanyName = details?.Name ?? bar.Ticker,
            Price = bar.Close,
            Change = bar.Change,
            ChangePercent = bar.ChangePercent,
            Open = bar.Open,
            High = bar.High,
            Low = bar.Low,
            Volume = bar.Volume,
            MarketCap = details?.FormattedMarketCap ?? "N/A",
            LastUpdated = bar.Date,
            Exchange = details?.PrimaryExchange ?? "N/A",
            Description = details?.Description ?? "No description available"
        };
    }
}
