using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Pnl;

public record MirrorPortfolio(
    [property: JsonPropertyName("mirrorId")]                  int MirrorId,
    [property: JsonPropertyName("availableAmount")]           decimal AvailableAmount,
    [property: JsonPropertyName("closedPositionsNetProfit")]  decimal ClosedPositionsNetProfit,
    [property: JsonPropertyName("positions")]                 IReadOnlyList<MirrorPosition> Positions,
    [property: JsonPropertyName("ordersForOpen")]             IReadOnlyList<OrderForOpen>? OrdersForOpen = null,
    [property: JsonPropertyName("ordersForClose")]            IReadOnlyList<OrderForClose>? OrdersForClose = null,
    [property: JsonPropertyName("ordersForCloseMultiple")]    IReadOnlyList<OrderForCloseMultiple>? OrdersForCloseMultiple = null,
    [property: JsonPropertyName("cid")]                       int Cid = 0,
    [property: JsonPropertyName("parentCID")]                 int ParentCid = 0,
    [property: JsonPropertyName("isPaused")]                  bool IsPaused = false,
    [property: JsonPropertyName("pendingForClosure")]         bool PendingForClosure = false,
    [property: JsonPropertyName("copyExistingPositions")]     bool CopyExistingPositions = false,
    [property: JsonPropertyName("initialInvestment")]         decimal InitialInvestment = 0,
    [property: JsonPropertyName("depositSummary")]            decimal DepositSummary = 0,
    [property: JsonPropertyName("withdrawalSummary")]         decimal WithdrawalSummary = 0,
    [property: JsonPropertyName("stopLossAmount")]            decimal StopLossAmount = 0,
    [property: JsonPropertyName("stopLossPercentage")]        decimal? StopLossPercentage = null,
    [property: JsonPropertyName("startedCopyDate")]           DateTimeOffset? StartedCopyDate = null,
    [property: JsonPropertyName("parentUsername")]            string? ParentUsername = null,
    [property: JsonPropertyName("mirrorStatusID")]            int MirrorStatusId = 0,
    [property: JsonPropertyName("mirrorCalculationType")]     int MirrorCalculationType = 0,
    [property: JsonPropertyName("parentMirrors")]             IReadOnlyList<JsonElement>? ParentMirrors = null,
    [property: JsonPropertyName("delayedOrderForClose")]      IReadOnlyList<JsonElement>? DelayedOrderForClose = null,
    [property: JsonPropertyName("delayedOrderForOpen")]       IReadOnlyList<JsonElement>? DelayedOrderForOpen = null,
    [property: JsonPropertyName("entryOrders")]               IReadOnlyList<JsonElement>? EntryOrders = null,
    [property: JsonPropertyName("exitOrders")]                IReadOnlyList<JsonElement>? ExitOrders = null
);
