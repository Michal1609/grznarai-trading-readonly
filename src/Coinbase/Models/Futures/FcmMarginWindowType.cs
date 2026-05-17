namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Known <c>margin_window_type</c> values within the FCM margin window measure.
/// </summary>
public static class FcmMarginWindowType
{
    public const string Overnight = "FCM_MARGIN_WINDOW_TYPE_OVERNIGHT";
    public const string Weekend = "FCM_MARGIN_WINDOW_TYPE_WEEKEND";
    public const string Intraday = "FCM_MARGIN_WINDOW_TYPE_INTRADAY";
    public const string Transition = "FCM_MARGIN_WINDOW_TYPE_TRANSITION";
}
