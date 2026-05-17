using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// Asset metadata returned as part of a portfolio balance entry.
/// </summary>
public sealed class PortfolioBalanceAsset
{
    [JsonPropertyName("asset_id")]
    public string? AssetId { get; set; }

    [JsonPropertyName("asset_uuid")]
    public string? AssetUuid { get; set; }

    [JsonPropertyName("asset_name")]
    public string? AssetName { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("collateral_weight")]
    public string? CollateralWeight { get; set; }

    [JsonPropertyName("account_collateral_limit")]
    public string? AccountCollateralLimit { get; set; }

    [JsonPropertyName("ecosystem_collateral_limit_breached")]
    public bool? EcosystemCollateralLimitBreached { get; set; }

    [JsonPropertyName("asset_icon_url")]
    public string? AssetIconUrl { get; set; }

    [JsonPropertyName("supported_networks_enabled")]
    public bool? SupportedNetworksEnabled { get; set; }
}
