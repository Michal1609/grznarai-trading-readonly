using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Common;
using GrznarAi.Trading.ReadOnly.Coinbase.Models.Fees;
using Xunit;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Tests.Integration;

/// <summary>
/// Manual integration tests against the real Coinbase Advanced Trade API.
/// Run with: dotnet test --filter "Category=Integration"
/// Requires: CoinbaseOptions__KeyName + CoinbaseOptions__PrivateKeyPem env vars,
///           or a local appsettings.test.json in the test project directory.
/// </summary>
[Trait("Category", "Integration")]
public class CoinbaseFeesIntegrationTests
{
    private ICoinbaseClient? _client;
    private ICoinbaseClient Client => _client ??= IntegrationTestSupport.CreateClient();

    // ─── GetTransactionSummaryAsync ──────────────────────────────────────────

    [CoinbaseIntegrationFact]
    public async Task GetTransactionSummary_returns_fee_tier()
    {
        var result = await Client.GetTransactionSummaryAsync();

        Debug.WriteLine($"TotalFees:              {result.TotalFees}");
        Debug.WriteLine($"FeeTier.PricingTier:    {result.FeeTier?.PricingTier}");
        Debug.WriteLine($"FeeTier.TakerFeeRate:   {result.FeeTier?.TakerFeeRate}");
        Debug.WriteLine($"FeeTier.MakerFeeRate:   {result.FeeTier?.MakerFeeRate}");
        Debug.WriteLine($"AdvTradeOnlyVolume:     {result.AdvancedTradeOnlyVolume}");
        Debug.WriteLine($"AdvTradeOnlyFees:       {result.AdvancedTradeOnlyFees}");
        Debug.WriteLine($"TotalBalance:           {result.TotalBalance}");
        Debug.WriteLine($"HasCostPlusCommission:  {result.HasCostPlusCommission}");

        Assert.NotNull(result.FeeTier);
    }

    [CoinbaseIntegrationFact]
    public async Task GetTransactionSummary_spot_filter_returns_result()
    {
        var result = await Client.GetTransactionSummaryAsync(productType: ProductType.Spot);

        Debug.WriteLine($"TotalFees (SPOT): {result.TotalFees}");
        Debug.WriteLine($"FeeTier:          {result.FeeTier?.PricingTier}");

        Assert.NotNull(result);
    }

    [CoinbaseIntegrationFact]
    public async Task GetTransactionSummary_request_overload_matches_params_overload()
    {
        var byParams = await Client.GetTransactionSummaryAsync(productType: ProductType.Spot);
        var byRequest = await Client.GetTransactionSummaryAsync(
            new GetTransactionSummaryRequest { ProductType = ProductType.Spot });

        Assert.Equal(byParams.TotalFees, byRequest.TotalFees);
        Assert.Equal(byParams.FeeTier?.PricingTier, byRequest.FeeTier?.PricingTier);
    }

    [CoinbaseIntegrationFact]
    public async Task GetTransactionSummary_volume_breakdown_printed()
    {
        var result = await Client.GetTransactionSummaryAsync();

        foreach (var vol in result.VolumeBreakdown ?? [])
            Debug.WriteLine($"  VolumeType={vol.VolumeType} Volume={vol.Volume}");

        Assert.NotNull(result);
    }

    [CoinbaseIntegrationFact]
    public async Task GetTransactionSummary_gst_printed_when_present()
    {
        var result = await Client.GetTransactionSummaryAsync();

        if (result.GoodsAndServicesTax is not null)
        {
            Debug.WriteLine($"GST Rate: {result.GoodsAndServicesTax.Rate}");
            Debug.WriteLine($"GST Type: {result.GoodsAndServicesTax.Type}");
        }
        else
        {
            Debug.WriteLine("GST not applicable for this account.");
        }

        Assert.NotNull(result);
    }
}
