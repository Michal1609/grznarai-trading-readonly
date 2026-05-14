using System.Diagnostics;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.RateLimiting;

namespace GrznarAi.Trading.ReadOnly.Tests.RateLimiting;

[TestFixture]
public class KeyedRateLimiterTests
{
    [Test]
    public async Task WaitAsync_DifferentCredentialKeys_DoNotBlockEachOther()
    {
        using var limiter = BuildLimiter(TimeSpan.FromSeconds(10));

        await limiter.WaitAsync(CreateRequest("account-a"), CancellationToken.None);

        var stopwatch = Stopwatch.StartNew();
        await limiter.WaitAsync(CreateRequest("account-b"), CancellationToken.None);
        stopwatch.Stop();

        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromMilliseconds(500)));
    }

    [Test]
    [Category("Timing")]
    public async Task WaitAsync_SameCredentialKey_SharesPermitWindow()
    {
        using var limiter = BuildLimiter(TimeSpan.FromMilliseconds(75));

        await limiter.WaitAsync(CreateRequest("account-a"), CancellationToken.None);

        var stopwatch = Stopwatch.StartNew();
        await limiter.WaitAsync(CreateRequest("account-a"), CancellationToken.None);
        stopwatch.Stop();

        Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(50)));
    }

    private static KeyedRateLimiter BuildLimiter(TimeSpan window) =>
        new(
            new RateLimitOptions
            {
                Enabled = true,
                PermitLimit = 1,
                Window = window
            },
            request => request.Headers.GetValues("X-Credential-Key").Single());

    private static HttpRequestMessage CreateRequest(string credentialKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test.example.com/");
        request.Headers.TryAddWithoutValidation("X-Credential-Key", credentialKey);
        return request;
    }
}

