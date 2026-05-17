namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;

/// <summary>
/// Known <c>margin_level</c> values indicating current margin health within a margin window.
/// </summary>
public static class MarginLevelType
{
    public const string Base = "MARGIN_LEVEL_TYPE_BASE";
    public const string Warning = "MARGIN_LEVEL_TYPE_WARNING";
    public const string Danger = "MARGIN_LEVEL_TYPE_DANGER";
    public const string Liquidation = "MARGIN_LEVEL_TYPE_LIQUIDATION";
}
