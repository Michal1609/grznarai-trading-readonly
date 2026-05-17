using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

[JsonConverter(typeof(JsonStringEnumConverter<PortfolioMarginFlags>))]
public enum PortfolioMarginFlags
{
    [JsonStringEnumMemberName("PORTFOLIO_MARGIN_FLAGS_UNSPECIFIED")] Unspecified = 0,
    [JsonStringEnumMemberName("PORTFOLIO_MARGIN_FLAGS_IN_LIQUIDATION")] InLiquidation = 1
}
