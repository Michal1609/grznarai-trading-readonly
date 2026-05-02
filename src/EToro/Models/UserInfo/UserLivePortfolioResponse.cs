using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.UserInfo;

public record UserLivePortfolioResponse(
    [property: JsonPropertyName("realizedCreditPct")] decimal RealizedCreditPct,
    [property: JsonPropertyName("unrealizedCreditPct")] decimal UnrealizedCreditPct,
    [property: JsonPropertyName("positions")] IReadOnlyList<UserLivePortfolioPosition>? Positions,
    [property: JsonPropertyName("socialTrades")] IReadOnlyList<UserLiveSocialTrade>? SocialTrades
);

public record UserLivePortfolioPosition(
    [property: JsonPropertyName("positionId")] long PositionId,
    [property: JsonPropertyName("openTimestamp")] DateTimeOffset? OpenTimestamp,
    [property: JsonPropertyName("openRate")] decimal OpenRate,
    [property: JsonPropertyName("instrumentId")] int InstrumentId,
    [property: JsonPropertyName("isBuy")] bool IsBuy,
    [property: JsonPropertyName("leverage")] int Leverage,
    [property: JsonPropertyName("takeProfitRate")] decimal? TakeProfitRate,
    [property: JsonPropertyName("stopLossRate")] decimal? StopLossRate,
    [property: JsonPropertyName("socialTradeId")] long? SocialTradeId,
    [property: JsonPropertyName("parentPositionId")] long? ParentPositionId,
    [property: JsonPropertyName("investmentPct")] decimal InvestmentPct,
    [property: JsonPropertyName("netProfit")] decimal NetProfit,
    [property: JsonPropertyName("trailingStopLoss")] bool TrailingStopLoss
);

public record UserLiveSocialTrade(
    [property: JsonPropertyName("socialTradeId")] long SocialTradeId,
    [property: JsonPropertyName("parentUsername")] string? ParentUsername,
    [property: JsonPropertyName("stopLossPercentage")] decimal StopLossPercentage,
    [property: JsonPropertyName("openTimestamp")] DateTimeOffset? OpenTimestamp,
    [property: JsonPropertyName("investmentPct")] decimal InvestmentPct,
    [property: JsonPropertyName("openInvestmentPct")] decimal OpenInvestmentPct,
    [property: JsonPropertyName("netProfit")] decimal NetProfit,
    [property: JsonPropertyName("openNetProfit")] decimal OpenNetProfit,
    [property: JsonPropertyName("closedNetProfit")] decimal ClosedNetProfit,
    [property: JsonPropertyName("realizedPct")] decimal RealizedPct,
    [property: JsonPropertyName("unrealizedPct")] decimal UnrealizedPct,
    [property: JsonPropertyName("isClosing")] bool IsClosing,
    [property: JsonPropertyName("positions")] IReadOnlyList<UserLivePortfolioPosition>? Positions
);
