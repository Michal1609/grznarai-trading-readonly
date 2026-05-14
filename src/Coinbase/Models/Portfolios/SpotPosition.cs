using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;
using GrznarAi.Trading.ReadOnly.Json;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

public sealed class SpotPosition
{
    [JsonPropertyName("asset")] public string? Asset { get; set; }
    [JsonPropertyName("account_uuid")] public string? AccountUuid { get; set; }

    [JsonPropertyName("total_balance_fiat")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? TotalBalanceFiat { get; set; }

    [JsonPropertyName("total_balance_crypto")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? TotalBalanceCrypto { get; set; }

    [JsonPropertyName("available_to_trade_fiat")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? AvailableToTradeFiat { get; set; }

    [JsonPropertyName("allocation")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? Allocation { get; set; }

    [JsonPropertyName("one_day_change")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? OneDayChange { get; set; }

    [JsonPropertyName("cost_basis")] public MoneyValue? CostBasis { get; set; }

    [JsonPropertyName("asset_img_url")] public string? AssetImgUrl { get; set; }

    [JsonPropertyName("is_cash")] public bool? IsCash { get; set; }

    [JsonPropertyName("average_entry_price")] public MoneyValue? AverageEntryPrice { get; set; }

    [JsonPropertyName("asset_uuid")] public string? AssetUuid { get; set; }

    [JsonPropertyName("available_to_trade_crypto")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? AvailableToTradeCrypto { get; set; }

    [JsonPropertyName("unrealized_pnl")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? UnrealizedPnl { get; set; }

    [JsonPropertyName("available_to_transfer")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? AvailableToTransfer { get; set; }

    [JsonPropertyName("available_to_transfer_fiat")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? AvailableToTransferFiat { get; set; }

    [JsonPropertyName("available_to_transfer_crypto")]
    [JsonConverter(typeof(NullableDecimalStringConverter))]
    public decimal? AvailableToTransferCrypto { get; set; }
}
