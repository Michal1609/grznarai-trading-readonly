using GrznarAi.Trading.ReadOnly.Configuration;
using Microsoft.Extensions.Options;

namespace GrznarAi.Trading.ReadOnly.Client;

/// <summary>
/// Sliding-window rate limiter for eToro API requests.
/// </summary>
/// <remarks>
/// Registered as a singleton — shared across all requests for a single eToro user key.
/// Multi-user scenarios (one application, multiple user keys) require a keyed or per-user
/// limiter instance instead of this shared singleton. Use <see cref="EToroKeyedRateLimiter"/>
/// and register it via <c>AddEToro(..., useKeyedRateLimiter: true)</c>.
/// <para>
/// Rate limit parameters (<see cref="RateLimitOptions.PermitLimit"/>, <see cref="RateLimitOptions.Window"/>)
/// are read once at construction time. Runtime changes via <c>IOptionsMonitor</c> are not
/// reflected; restart the application to apply new rate-limit configuration.
/// </para>
/// </remarks>
public sealed class EToroRateLimiter : IEToroRateLimiter, IDisposable
{
    private readonly EToroSlidingWindow _window;
    private readonly RateLimitOptions _options;

    public EToroRateLimiter(IOptions<EToroOptions> options)
    {
        _options = options.Value.RateLimit;
        _window = new EToroSlidingWindow(_options.PermitLimit, _options.Window, TimeProvider.System);
    }

    internal EToroRateLimiter(IOptions<EToroOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value.RateLimit;
        _window = new EToroSlidingWindow(_options.PermitLimit, _options.Window, timeProvider);
    }

    public async Task WaitAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (!_options.Enabled || request.Method != HttpMethod.Get)
            return;

        await _window.WaitAsync(ct).ConfigureAwait(false);
    }

    public void Dispose() => _window.Dispose();
}
