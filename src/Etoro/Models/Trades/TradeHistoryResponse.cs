namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Trades;

/// <summary>
/// Represents the trade history endpoint response.
/// </summary>
/// <remarks>
/// The eToro API returns a raw <see cref="ClosedTrade"/> array without a wrapper object
/// or pagination metadata. This type keeps the public client contract explicit.
/// </remarks>
public record TradeHistoryResponse(IReadOnlyList<ClosedTrade> Trades);
