using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;
using GrznarAi.Trading.ReadOnly.Json;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

public sealed class FuturesPosition
{
    [JsonPropertyName("product_id")] public string? ProductId { get; set; }

    [JsonPropertyName("contract_size")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? ContractSize { get; set; }

    [JsonPropertyName("side")] public PositionSide? Side { get; set; }

    [JsonPropertyName("amount")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? Amount { get; set; }

    [JsonPropertyName("avg_entry_price")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? AvgEntryPrice { get; set; }

    [JsonPropertyName("current_price")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? CurrentPrice { get; set; }

    [JsonPropertyName("unrealized_pnl")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? UnrealizedPnl { get; set; }

    [JsonPropertyName("expiry")] public DateTimeOffset? Expiry { get; set; }

    [JsonPropertyName("underlying_asset_id")] public string? UnderlyingAssetId { get; set; }

    [JsonPropertyName("asset_img_url")] public string? AssetImgUrl { get; set; }

    [JsonPropertyName("product_name")] public string? ProductName { get; set; }

    [JsonPropertyName("venue")] public string? Venue { get; set; }

    [JsonPropertyName("notional_value")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? NotionalValue { get; set; }
}
