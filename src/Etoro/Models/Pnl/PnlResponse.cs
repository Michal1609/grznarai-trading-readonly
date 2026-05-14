using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Pnl;

public record PnlApiResponse(
    [property: JsonPropertyName("clientPortfolio")] PnlResponse ClientPortfolio
);

public record PnlResponse(
    [property: JsonPropertyName("credit")]                  decimal Credit,
    [property: JsonPropertyName("positions")]               IReadOnlyList<Position> Positions,
    [property: JsonPropertyName("mirrors")]                 IReadOnlyList<MirrorPortfolio> MirrorPortfolios,
    [property: JsonPropertyName("ordersForOpen")]           IReadOnlyList<OrderForOpen> OrdersForOpen,
    [property: JsonPropertyName("orders")]                  IReadOnlyList<MitOrder> Orders,
    [property: JsonPropertyName("bonusCredit")]             decimal BonusCredit = 0,
    [property: JsonPropertyName("unrealizedPnL")]           decimal UnrealizedPnL = 0,
    [property: JsonPropertyName("accountCurrencyId")]       int AccountCurrencyId = 0,
    [property: JsonPropertyName("ordersForClose")]          IReadOnlyList<OrderForClose>? OrdersForClose = null,
    [property: JsonPropertyName("ordersForCloseMultiple")]  IReadOnlyList<OrderForCloseMultiple>? OrdersForCloseMultiple = null,
    [property: JsonPropertyName("stockOrders")]             IReadOnlyList<JsonElement>? StockOrders = null,
    [property: JsonPropertyName("entryOrders")]             IReadOnlyList<JsonElement>? EntryOrders = null,
    [property: JsonPropertyName("exitOrders")]              IReadOnlyList<JsonElement>? ExitOrders = null
);
