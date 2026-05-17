namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;

/// <summary>
/// Known <c>product_venue</c> values used across Coinbase Advanced Trade API endpoints.
/// </summary>
public static class ProductVenue
{
    public const string Unknown = "UNKNOWN_VENUE_TYPE";

    /// <summary>Coinbase Exchange (spot).</summary>
    public const string Cbe = "CBE";

    /// <summary>Futures Commission Merchant (US derivatives).</summary>
    public const string Fcm = "FCM";

    /// <summary>International Exchange (perpetuals).</summary>
    public const string Intx = "INTX";
}
