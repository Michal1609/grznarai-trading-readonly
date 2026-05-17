using GrznarAi.Trading.ReadOnly.Coinbase.Models.Futures;
using GrznarAi.Trading.ReadOnly.Querying;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

public sealed partial class CoinbaseClient
{
    /// <inheritdoc cref="ICoinbaseFuturesClient.GetCurrentMarginWindowAsync(string?,CancellationToken)"/>
    public async Task<GetCurrentMarginWindowResponse> GetCurrentMarginWindowAsync(
        string? marginProfileType = null,
        CancellationToken ct = default)
    {
        var qs = new QueryStringBuilder()
            .Add("margin_profile_type", marginProfileType);

        return await GetFromJsonAsync<GetCurrentMarginWindowResponse>(
            $"/api/v3/brokerage/cfm/intraday/current_margin_window{qs}",
            "Empty get-current-margin-window response.",
            ct).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ICoinbaseFuturesClient.GetCurrentMarginWindowAsync(GetCurrentMarginWindowRequest,CancellationToken)"/>
    public Task<GetCurrentMarginWindowResponse> GetCurrentMarginWindowAsync(
        GetCurrentMarginWindowRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return GetCurrentMarginWindowAsync(request.MarginProfileType, ct);
    }

    /// <inheritdoc cref="ICoinbaseFuturesClient.GetFuturesBalanceSummaryAsync"/>
    public Task<GetFuturesBalanceSummaryResponse> GetFuturesBalanceSummaryAsync(
        CancellationToken ct = default)
        => GetFromJsonAsync<GetFuturesBalanceSummaryResponse>(
            "/api/v3/brokerage/cfm/balance_summary",
            "Empty get-futures-balance-summary response.",
            ct);

    /// <inheritdoc cref="ICoinbaseFuturesClient.GetFuturesPositionAsync"/>
    public Task<GetFuturesPositionResponse> GetFuturesPositionAsync(
        string productId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID must not be empty.", nameof(productId));

        var escapedId = QueryStringBuilder.EscapePathSegment(productId);

        return GetFromJsonAsync<GetFuturesPositionResponse>(
            $"/api/v3/brokerage/cfm/positions/{escapedId}",
            "Empty get-futures-position response.",
            ct);
    }

    /// <inheritdoc cref="ICoinbaseFuturesClient.GetIntradayMarginSettingAsync"/>
    public Task<GetIntradayMarginSettingResponse> GetIntradayMarginSettingAsync(
        CancellationToken ct = default)
        => GetFromJsonAsync<GetIntradayMarginSettingResponse>(
            "/api/v3/brokerage/cfm/intraday/margin_setting",
            "Empty get-intraday-margin-setting response.",
            ct);

    /// <inheritdoc cref="ICoinbaseFuturesClient.ListFuturesPositionsAsync"/>
    public Task<ListFuturesPositionsResponse> ListFuturesPositionsAsync(
        CancellationToken ct = default)
        => GetFromJsonAsync<ListFuturesPositionsResponse>(
            "/api/v3/brokerage/cfm/positions",
            "Empty list-futures-positions response.",
            ct);

    /// <inheritdoc cref="ICoinbaseFuturesClient.ListFuturesSweepsAsync"/>
    public Task<ListFuturesSweepsResponse> ListFuturesSweepsAsync(
        CancellationToken ct = default)
        => GetFromJsonAsync<ListFuturesSweepsResponse>(
            "/api/v3/brokerage/cfm/sweeps",
            "Empty list-futures-sweeps response.",
            ct);
}
