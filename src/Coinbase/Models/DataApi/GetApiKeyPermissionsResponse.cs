using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.DataApi;

/// <summary>
/// Response from <c>GET /api/v3/brokerage/key_permissions</c>.
/// </summary>
public sealed class GetApiKeyPermissionsResponse
{
    /// <summary>Whether the API key has view (read) permissions.</summary>
    [JsonPropertyName("can_view")]
    public bool? CanView { get; set; }

    /// <summary>Whether the API key has trade permissions.</summary>
    [JsonPropertyName("can_trade")]
    public bool? CanTrade { get; set; }

    /// <summary>Whether the API key has deposit/withdrawal (transfer) permissions.</summary>
    [JsonPropertyName("can_transfer")]
    public bool? CanTransfer { get; set; }

    /// <summary>Whether the API key permits receiving inbound payments.</summary>
    [JsonPropertyName("can_receive")]
    public bool? CanReceive { get; set; }

    /// <summary>UUID of the portfolio linked to this API key.</summary>
    [JsonPropertyName("portfolio_uuid")]
    public string? PortfolioUuid { get; set; }

    /// <summary>
    /// Category of the linked portfolio.
    /// See <see cref="PortfolioType"/> for known values.
    /// </summary>
    [JsonPropertyName("portfolio_type")]
    public string? PortfolioType { get; set; }
}
