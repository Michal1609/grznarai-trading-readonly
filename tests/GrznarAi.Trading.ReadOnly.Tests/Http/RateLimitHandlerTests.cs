using System.Net;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Http;
using GrznarAi.Trading.ReadOnly.RateLimiting;
using GrznarAi.Trading.ReadOnly.Tests.Helpers;

namespace GrznarAi.Trading.ReadOnly.Tests.Http;

[TestFixture]
public class RateLimitHandlerTests
{
    private sealed class NoOpRateLimiter : IApiRateLimiter
    {
        public Task WaitAsync(HttpRequestMessage request, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class TrackingResponse(HttpStatusCode statusCode) : HttpResponseMessage(statusCode)
    {
        public bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private static HttpClient BuildClient(
        SequentialHttpMessageHandler inner,
        RateLimitOptions? rateLimitOptions = null,
        Func<TimeSpan, CancellationToken, Task>? delay = null,
        IApiRateLimiter? rateLimiter = null)
    {
        var options = rateLimitOptions ?? new RateLimitOptions
        {
            Enabled = false,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryJitterRatio = 0
        };

        var handler = new RateLimitHandler(options, rateLimiter ?? new NoOpRateLimiter(), delay)
        {
            InnerHandler = inner
        };

        return new HttpClient(handler) { BaseAddress = new Uri("https://test.example.com/") };
    }

    [Test]
    public async Task NonRateLimited_ReturnsResponseImmediately()
    {
        var inner = new SequentialHttpMessageHandler(() => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner);

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task RateLimited_ThenSuccess_RetriesReturnsOkAndDisposes429()
    {
        var rateLimited = new TrackingResponse(HttpStatusCode.TooManyRequests);
        rateLimited.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(
            TimeSpan.FromMilliseconds(1));

        var inner = new SequentialHttpMessageHandler(
            () => rateLimited,
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner);

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
        Assert.That(rateLimited.WasDisposed, Is.True);
    }

    [Test]
    public async Task RateLimited_ExceedsMaxRetries_Returns429()
    {
        static HttpResponseMessage Make429()
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(
                TimeSpan.FromMilliseconds(1));
            return response;
        }

        var inner = new SequentialHttpMessageHandler(Make429, Make429, Make429, Make429);
        using var client = BuildClient(inner, new RateLimitOptions
        {
            Enabled = false,
            MaxRetries = 3,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryJitterRatio = 0
        });

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(inner.CallCount, Is.EqualTo(4));
    }

    [Test]
    public async Task RateLimited_MaxRetriesZero_Returns429WithoutRetry()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner, new RateLimitOptions
        {
            Enabled = false,
            MaxRetries = 0,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryJitterRatio = 0
        });

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task RateLimited_PostDoesNotRetryByDefault()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner, new RateLimitOptions
        {
            Enabled = false,
            MaxRetries = 3,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryJitterRatio = 0
        });

        using var content = new StringContent("payload");
        using var response = await client.PostAsync("/test", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task RateLimited_PostRetriesWhenNonIdempotentRetryPolicyIsExplicitlyEnabled()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner, new RateLimitOptions
        {
            Enabled = false,
            MaxRetries = 1,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryJitterRatio = 0,
            RetryNonIdempotentRequests = true
        });

        using var content = new StringContent("payload");
        using var response = await client.PostAsync("/test", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async Task RateLimited_HeadRetriesByDefault()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner, new RateLimitOptions
        {
            Enabled = false,
            MaxRetries = 1,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryJitterRatio = 0
        });

        using var request = new HttpRequestMessage(HttpMethod.Head, "/test");
        using var response = await client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async Task RateLimited_ClonedRequestPreservesVersionPolicyAndOptions()
    {
        var optionKey = new HttpRequestOptionsKey<string>("test-option");
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner, new RateLimitOptions
        {
            Enabled = false,
            MaxRetries = 1,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryJitterRatio = 0
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, "/test")
        {
            Version = HttpVersion.Version20,
            VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
        };
        request.Options.Set(optionKey, "value");

        using var response = await client.SendAsync(request);

        var secondRequest = inner.Requests[1];
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
        Assert.That(secondRequest.Version, Is.EqualTo(HttpVersion.Version20));
        Assert.That(secondRequest.VersionPolicy, Is.EqualTo(HttpVersionPolicy.RequestVersionOrHigher));
        Assert.That(secondRequest.Options.TryGetValue(optionKey, out var optionValue), Is.True);
        Assert.That(optionValue, Is.EqualTo("value"));
    }

    [Test]
    public async Task RateLimited_WithRetryAfterDelta_UsesHeaderDelay()
    {
        var delays = new List<TimeSpan>();
        var inner = new SequentialHttpMessageHandler(
            () =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(
                    TimeSpan.FromMilliseconds(50));
                return response;
            },
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(
            inner,
            new RateLimitOptions
            {
                Enabled = false,
                DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
                MaxRetryDelay = TimeSpan.FromSeconds(1),
                RetryJitterRatio = 0
            },
            (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(delays, Is.EqualTo(new[] { TimeSpan.FromMilliseconds(50) }));
    }

    [Test]
    public async Task RateLimited_NoRetryAfterHeader_UsesDefaultBackoffDelay()
    {
        var delays = new List<TimeSpan>();
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(
            inner,
            new RateLimitOptions
            {
                Enabled = false,
                DefaultRetryDelay = TimeSpan.FromMilliseconds(25),
                MaxRetryDelay = TimeSpan.FromSeconds(1),
                RetryJitterRatio = 0
            },
            (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(delays, Is.EqualTo(new[] { TimeSpan.FromMilliseconds(25) }));
    }

    [Test]
    public async Task RateLimited_RetryAfterLongerThanMaxDelay_IsCapped()
    {
        var delays = new List<TimeSpan>();
        var inner = new SequentialHttpMessageHandler(
            () =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(
                    TimeSpan.FromMinutes(10));
                return response;
            },
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(
            inner,
            new RateLimitOptions
            {
                Enabled = false,
                DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
                MaxRetryDelay = TimeSpan.FromMilliseconds(75),
                RetryJitterRatio = 0
            },
            (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(delays, Is.EqualTo(new[] { TimeSpan.FromMilliseconds(75) }));
    }

    [Test]
    public void RateLimited_DelayCancellation_ThrowsTaskCanceledException()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(
            inner,
            new RateLimitOptions
            {
                Enabled = false,
                DefaultRetryDelay = TimeSpan.FromSeconds(60),
                MaxRetryDelay = TimeSpan.FromSeconds(60),
                RetryJitterRatio = 0
            });

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await client.GetAsync("/test", cts.Token));
    }

    [Test]
    [Category("Timing")]
    public async Task ProactiveGetLimiter_WaitsWhenPermitWindowIsExhausted()
    {
        var options = new RateLimitOptions
        {
            Enabled = true,
            PermitLimit = 1,
            Window = TimeSpan.FromMilliseconds(75),
            MaxRetries = 1,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryJitterRatio = 0
        };
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.OK),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner, options, rateLimiter: new ApiRateLimiter(options));

        using var first = await client.GetAsync("/test");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        using var second = await client.GetAsync("/test");
        stopwatch.Stop();

        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(50)));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async Task OtherErrors_AreNotRetried()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.Unauthorized),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner);

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }
}

