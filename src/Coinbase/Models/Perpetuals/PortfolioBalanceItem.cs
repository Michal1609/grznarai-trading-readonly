using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Perpetuals;

/// <summary>
/// A single asset balance line within a perpetuals portfolio.
/// </summary>
public sealed class PortfolioBalanceItem
{
    [JsonPropertyName("asset")]
    public PortfolioBalanceAsset? Asset { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("hold")]
    public string? Hold { get; set; }

    [JsonPropertyName("transfer_hold")]
    public string? TransferHold { get; set; }

    [JsonPropertyName("collateral_value")]
    public string? CollateralValue { get; set; }

    [JsonPropertyName("collateral_weight")]
    public string? CollateralWeight { get; set; }

    [JsonPropertyName("max_withdraw_amount")]
    public string? MaxWithdrawAmount { get; set; }

    [JsonPropertyName("loan")]
    public string? Loan { get; set; }

    [JsonPropertyName("loan_collateral_requirement_usd")]
    public string? LoanCollateralRequirementUsd { get; set; }

    [JsonPropertyName("pledged_quantity")]
    public string? PledgedQuantity { get; set; }

    [JsonPropertyName("max_portfolio_transfer_amount")]
    public string? MaxPortfolioTransferAmount { get; set; }
}
