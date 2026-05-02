using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Pnl;

public record PositionUnrealizedPnL(
    [property: JsonPropertyName("pnL")]                      decimal PnL,
    [property: JsonPropertyName("pnlAssetCurrency")]         decimal PnlAssetCurrency,
    [property: JsonPropertyName("exposureInAccountCurrency")]decimal ExposureInAccountCurrency,
    [property: JsonPropertyName("exposureInAssetCurrency")]  decimal ExposureInAssetCurrency,
    [property: JsonPropertyName("marginInAccountCurrency")]  decimal MarginInAccountCurrency,
    [property: JsonPropertyName("marginInAssetCurrency")]    decimal MarginInAssetCurrency,
    [property: JsonPropertyName("marginCurrencyId")]         int MarginCurrencyId = 0,
    [property: JsonPropertyName("assetCurrencyId")]          int AssetCurrencyId = 0,
    [property: JsonPropertyName("closeRate")]                decimal CloseRate = 0,
    [property: JsonPropertyName("closeConversionRate")]      decimal CloseConversionRate = 1,
    [property: JsonPropertyName("timestamp")]                DateTimeOffset? Timestamp = null
);
