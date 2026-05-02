using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Pnl;

public record MirrorPortfolio(
    [property: JsonPropertyName("mirrorId")]                  int MirrorId,
    [property: JsonPropertyName("availableAmount")]           decimal AvailableAmount,
    [property: JsonPropertyName("closedPositionsNetProfit")]  decimal ClosedPositionsNetProfit,
    [property: JsonPropertyName("positions")]                 IReadOnlyList<MirrorPosition> Positions,
    [property: JsonPropertyName("ordersForOpen")]             IReadOnlyList<OrderForOpen>? OrdersForOpen = null,
    [property: JsonPropertyName("cid")]                       int Cid = 0,
    [property: JsonPropertyName("isPaused")]                  bool IsPaused = false,
    [property: JsonPropertyName("initialInvestment")]         decimal InitialInvestment = 0,
    [property: JsonPropertyName("depositSummary")]            decimal DepositSummary = 0,
    [property: JsonPropertyName("withdrawalSummary")]         decimal WithdrawalSummary = 0,
    [property: JsonPropertyName("stopLossPercentage")]        decimal? StopLossPercentage = null,
    [property: JsonPropertyName("startedCopyDate")]           DateTimeOffset? StartedCopyDate = null
);
