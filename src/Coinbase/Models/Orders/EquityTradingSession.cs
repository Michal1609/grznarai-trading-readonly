namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>equity_trading_session</c> values returned by the Coinbase API.</summary>
public static class EquityTradingSession
{
    public const string Unknown = "UNKNOWN_EQUITY_TRADING_SESSION";
    public const string Normal = "EQUITY_TRADING_SESSION_NORMAL";
    public const string AfterHours = "EQUITY_TRADING_SESSION_AFTER_HOURS";
    public const string MultiSession = "EQUITY_TRADING_SESSION_MULTI_SESSION";
    public const string Overnight = "EQUITY_TRADING_SESSION_OVERNIGHT";
    public const string PreMarket = "EQUITY_TRADING_SESSION_PRE_MARKET";
}
