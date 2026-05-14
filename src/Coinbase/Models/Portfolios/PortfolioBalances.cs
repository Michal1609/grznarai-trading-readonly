using System.Text.Json.Serialization;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

public sealed class PortfolioBalances
{
    [JsonPropertyName("total_balance")] public MoneyValue? TotalBalance { get; set; }
    [JsonPropertyName("total_futures_balance")] public MoneyValue? TotalFuturesBalance { get; set; }
    [JsonPropertyName("total_cash_equivalent_balance")] public MoneyValue? TotalCashEquivalentBalance { get; set; }
    [JsonPropertyName("total_crypto_balance")] public MoneyValue? TotalCryptoBalance { get; set; }
    [JsonPropertyName("futures_unrealized_pnl")] public MoneyValue? FuturesUnrealizedPnl { get; set; }
    [JsonPropertyName("perp_unrealized_pnl")] public MoneyValue? PerpUnrealizedPnl { get; set; }
}
