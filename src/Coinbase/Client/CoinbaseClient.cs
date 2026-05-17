using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Client;

/// <summary>
/// Typed read-oriented client for the Coinbase Advanced Trade API.
/// </summary>
/// <remarks>
/// This project is not affiliated with, endorsed by, or connected to Coinbase.
/// It does not provide financial advice; see the README disclaimer before using the client.
/// </remarks>
public sealed partial class CoinbaseClient : ICoinbaseClient
{
    public const string HttpClientName = "GrznarAi.Trading.ReadOnly.Coinbase";

    private static readonly CoinbaseJsonContext JsonContext = CoinbaseJsonContext.Default;

    private readonly HttpClient _httpClient;
    private readonly ErrorHandlingOptions _errorHandlingOptions;
    private readonly ILogger<CoinbaseClient> _logger;

    internal CoinbaseClient(HttpClient httpClient)
        : this(httpClient, Options.Create(new CoinbaseOptions()))
    {
    }

    public CoinbaseClient(HttpClient httpClient, IOptions<CoinbaseOptions> options)
        : this(httpClient, options, NullLogger<CoinbaseClient>.Instance)
    {
    }

    [ActivatorUtilitiesConstructor]
    public CoinbaseClient(
        HttpClient httpClient,
        IOptions<CoinbaseOptions> options,
        ILogger<CoinbaseClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _errorHandlingOptions = options.Value.ErrorHandling ?? new ErrorHandlingOptions();
        _logger = logger;
    }

    private async Task<T> GetFromJsonAsync<T>(
        string url,
        string emptyResponseMessage,
        CancellationToken ct)
    {
        LogSendingRequest(_logger, "GET", url);
        using var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
        await EnsureSuccessAsync(response, ct).ConfigureAwait(false);

        using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync(stream, GetJsonTypeInfo<T>(), ct).ConfigureAwait(false)
               ?? throw new InvalidOperationException(emptyResponseMessage);
    }

    private static JsonTypeInfo<T> GetJsonTypeInfo<T>() =>
        JsonContext.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
        ?? throw new InvalidOperationException($"JSON type metadata for {typeof(T).FullName} is not registered.");

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        string? rawBody = null;
        if (_errorHandlingOptions.IncludeResponseBody && response.Content is not null)
        {
            rawBody = _errorHandlingOptions.MaxResponseBodyLength is { } maxBytes
                ? await ReadBoundedBodyAsync(response.Content, maxBytes, ct).ConfigureAwait(false)
                : await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }

        var body = rawBody is null ? null : FormatResponseBody(rawBody, _errorHandlingOptions);
        var endpoint = GetSanitizedEndpoint(response);
        var requestId = GetRequestId(response);
        var retryAfter = GetRetryAfter(response);
        LogRequestFailed(_logger, (int)response.StatusCode, endpoint, requestId, retryAfter);

        throw new CoinbaseApiException(
            response.StatusCode,
            endpoint,
            body,
            requestId,
            retryAfter);
    }

    private static async Task<string> ReadBoundedBodyAsync(HttpContent content, int maxBytes, CancellationToken ct)
    {
        using var stream = await content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var buffer = new byte[maxBytes];
        var total = 0;
        int read;
        while (total < maxBytes
               && (read = await stream.ReadAsync(buffer.AsMemory(total, maxBytes - total), ct).ConfigureAwait(false)) > 0)
            total += read;
        return System.Text.Encoding.UTF8.GetString(buffer, 0, total);
    }

    private static string GetSanitizedEndpoint(HttpResponseMessage response)
    {
        var requestUri = response.RequestMessage?.RequestUri;
        if (requestUri is null)
            return "<unknown>";

        var path = requestUri.IsAbsoluteUri ? requestUri.AbsolutePath : requestUri.OriginalString;
        path = PortfolioPathRegex().Replace(path, "/portfolios/{uuid}");
        path = AccountPathRegex().Replace(path, "/accounts/{uuid}");
        path = ConvertTradePathRegex().Replace(path, "/convert/trade/{trade_id}");
        path = OrderPathRegex().Replace(path, "/orders/historical/{order_id}");
        path = PaymentMethodPathRegex().Replace(path, "/payment_methods/{payment_method_id}");
        path = IntxPortfolioPathRegex().Replace(path, "/intx/portfolio/{portfolio_uuid}");
        path = IntxBalancesPathRegex().Replace(path, "/intx/balances/{portfolio_uuid}");
        path = IntxPositionPathRegex().Replace(path, "/intx/positions/{portfolio_uuid}/{symbol}");
        path = IntxPositionsPathRegex().Replace(path, "/intx/positions/{portfolio_uuid}");
        path = ProductCandlesPathRegex().Replace(path, "/products/{product_id}/candles");
        path = ProductTickerPathRegex().Replace(path, "/products/{product_id}/ticker");
        path = ProductPathRegex().Replace(path, "/products/{product_id}");
        return path;
    }

    private static string? GetRequestId(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-request-id", out var requestIds))
            return requestIds.FirstOrDefault();

        if (response.Headers.TryGetValues("cb-trace-id", out requestIds))
            return requestIds.FirstOrDefault();

        return null;
    }

    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;

        if (retryAfter?.Delta is { } delta)
            return delta;

        if (retryAfter?.Date is { } date)
        {
            var wait = date - DateTimeOffset.UtcNow;
            return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
        }

        return null;
    }

    private static string FormatResponseBody(string body, ErrorHandlingOptions options)
    {
        var formatted = body.Trim();

        if (options.RedactResponseBody)
        {
            formatted = JsonSecretRegex().Replace(formatted, "${prefix}\"[REDACTED]\"");
            formatted = TextSecretRegex().Replace(formatted, "${prefix}[REDACTED]");
            formatted = BearerTokenRegex().Replace(formatted, "Bearer [REDACTED]");
            formatted = JwtTokenRegex().Replace(formatted, "[REDACTED_JWT]");
        }

        return options.MaxResponseBodyLength is { } maxLength && formatted.Length > maxLength
            ? formatted[..maxLength]
            : formatted;
    }

    [GeneratedRegex(
        "(?<prefix>[\"']?(?:apiKey|api_key|privateKey|private_key|token|accessToken|refreshToken|access_token|refresh_token|authorization|secret|password)[\"']?\\s*:\\s*)[\"'](?:\\\\.|[^\"'\\\\])*[\"']",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex JsonSecretRegex();

    [GeneratedRegex(
        "(?<prefix>\\b(?:apiKey|api_key|privateKey|private_key|token|accessToken|refreshToken|access_token|refresh_token|authorization|secret|password)\\b\\s*[=:]\\s*)[^\\s,;}&\\]]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TextSecretRegex();

    [GeneratedRegex(
        @"Bearer\s+[A-Za-z0-9._~+/\-]+=*",
        RegexOptions.CultureInvariant)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(
        @"eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex JwtTokenRegex();

    [GeneratedRegex(
        @"/portfolios/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex PortfolioPathRegex();

    [GeneratedRegex(
        @"/accounts/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex AccountPathRegex();

    [GeneratedRegex(
        @"/convert/trade/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex ConvertTradePathRegex();

    [GeneratedRegex(
        @"/orders/historical/(?!batch|fills)[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex OrderPathRegex();

    [GeneratedRegex(
        @"/payment_methods/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex PaymentMethodPathRegex();

    [GeneratedRegex(
        @"/intx/portfolio/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex IntxPortfolioPathRegex();

    [GeneratedRegex(
        @"/intx/balances/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex IntxBalancesPathRegex();

    [GeneratedRegex(
        @"/intx/positions/[^/?#]+/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex IntxPositionPathRegex();

    [GeneratedRegex(
        @"/intx/positions/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex IntxPositionsPathRegex();

    [GeneratedRegex(
        @"/products/[^/?#]+/candles",
        RegexOptions.CultureInvariant)]
    private static partial Regex ProductCandlesPathRegex();

    [GeneratedRegex(
        @"/products/[^/?#]+/ticker",
        RegexOptions.CultureInvariant)]
    private static partial Regex ProductTickerPathRegex();

    [GeneratedRegex(
        @"/products/[^/?#]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex ProductPathRegex();

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "Sending Coinbase request. Method: {Method}; Endpoint: {Endpoint}.")]
    private static partial void LogSendingRequest(
        ILogger logger,
        string method,
        string endpoint);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Warning,
        Message = "Coinbase request failed. StatusCode: {StatusCode}; Endpoint: {Endpoint}; RequestId: {RequestId}; RetryAfter: {RetryAfter}.")]
    private static partial void LogRequestFailed(
        ILogger logger,
        int statusCode,
        string endpoint,
        string? requestId,
        TimeSpan? retryAfter);
}
