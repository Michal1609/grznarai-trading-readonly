namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Known <c>trade_type</c> values for fills returned by the Coinbase API.</summary>
public static class TradeType
{
    public const string Fill = "FILL";
    public const string Reversal = "REVERSAL";
    public const string Correction = "CORRECTION";
    public const string Synthetic = "SYNTHETIC";
}
