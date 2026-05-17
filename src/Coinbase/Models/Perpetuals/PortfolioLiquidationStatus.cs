using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

[JsonConverter(typeof(JsonStringEnumConverter<PortfolioLiquidationStatus>))]
public enum PortfolioLiquidationStatus
{
    [JsonStringEnumMemberName("PORTFOLIO_LIQUIDATION_STATUS_UNSPECIFIED")] Unspecified = 0,
    [JsonStringEnumMemberName("PORTFOLIO_LIQUIDATION_STATUS_NOT_LIQUIDATING")] NotLiquidating = 1,
    [JsonStringEnumMemberName("PORTFOLIO_LIQUIDATION_STATUS_CANCELING")] Canceling = 2,
    [JsonStringEnumMemberName("PORTFOLIO_LIQUIDATION_STATUS_AUTO_LIQUIDATING")] AutoLiquidating = 3,
    [JsonStringEnumMemberName("PORTFOLIO_LIQUIDATION_STATUS_LSP_ASSIGNMENT")] LspAssignment = 4,
    [JsonStringEnumMemberName("PORTFOLIO_LIQUIDATION_STATUS_CUSTOMER_ASSIGNMENT")] CustomerAssignment = 5,
    [JsonStringEnumMemberName("PORTFOLIO_LIQUIDATION_STATUS_MANUAL")] Manual = 6
}
