using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Pnl;

public record PnlApiResponse(
    [property: JsonPropertyName("clientPortfolio")] PnlResponse ClientPortfolio
);

public record PnlResponse(
    [property: JsonPropertyName("credit")]          decimal Credit,
    [property: JsonPropertyName("positions")]        IReadOnlyList<Position> Positions,
    [property: JsonPropertyName("mirrors")]          IReadOnlyList<MirrorPortfolio> MirrorPortfolios,
    [property: JsonPropertyName("ordersForOpen")]    IReadOnlyList<OrderForOpen> OrdersForOpen,
    [property: JsonPropertyName("orders")]           IReadOnlyList<MitOrder> Orders,
    [property: JsonPropertyName("bonusCredit")]      decimal BonusCredit = 0,
    [property: JsonPropertyName("unrealizedPnL")]    decimal UnrealizedPnL = 0
);
