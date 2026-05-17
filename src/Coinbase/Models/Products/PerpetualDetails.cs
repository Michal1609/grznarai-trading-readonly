using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Perpetual contract details nested inside <see cref="FutureProductDetails"/>.</summary>
public sealed class PerpetualDetails
{
    [JsonPropertyName("open_interest")] public string? OpenInterest { get; set; }
    [JsonPropertyName("funding_rate")] public string? FundingRate { get; set; }
    [JsonPropertyName("funding_time")] public DateTimeOffset? FundingTime { get; set; }
    [JsonPropertyName("max_leverage")] public string? MaxLeverage { get; set; }
    [JsonPropertyName("base_asset_uuid")] public string? BaseAssetUuid { get; set; }
    [JsonPropertyName("underlying_type")] public string? UnderlyingType { get; set; }
}
