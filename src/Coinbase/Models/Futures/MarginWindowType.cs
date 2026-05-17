namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Known <c>margin_window_type</c> values returned by the current margin window endpoint.
/// </summary>
public static class MarginWindowType
{
    public const string Unspecified = "MARGIN_WINDOW_TYPE_UNSPECIFIED";
    public const string Overnight = "MARGIN_WINDOW_TYPE_OVERNIGHT";
    public const string Weekend = "MARGIN_WINDOW_TYPE_WEEKEND";
    public const string Intraday = "MARGIN_WINDOW_TYPE_INTRADAY";
    public const string Transition = "MARGIN_WINDOW_TYPE_TRANSITION";
}
