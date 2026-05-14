using System.Net;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Http;
using GrznarAi.Trading.ReadOnly.Tests.Helpers;

namespace GrznarAi.Trading.ReadOnly.Tests.Http;

[TestFixture]
public class TransientHttpErrorHandlerTests
{
    private static HttpClient BuildClient(
        SequentialHttpMessageHandler inner,
        ResilienceOptions? resilienceOptions = null,
        Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        var options = resilienceOptions ?? new ResilienceOptions
        {
            Enabled = true,
            MaxRetries = 2,
            DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryJitterRatio = 0
        };

        var handler = new TransientHttpErrorHandler(options, delay)
        {
            InnerHandler = inner
        };

        return new HttpClient(handler) { BaseAddress = new Uri("https://test.example.com/") };
    }

    [Test]
    public async Task TransientServerError_ThenSuccess_RetriesIdempotentRequest()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner);

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async Task TransientNetworkError_ThenSuccess_RetriesIdempotentRequest()
    {
        var inner = new SequentialHttpMessageHandler(
            () => throw new HttpRequestException("network failure"),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner);

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async Task TransientServerError_PostDoesNotRetryByDefault()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner);

        using var content = new StringContent("payload");
        using var response = await client.PostAsync("/test", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task TransientServerError_PostRetriesWhenNonIdempotentRetryPolicyIsExplicitlyEnabled()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner, new ResilienceOptions
        {
            Enabled = true,
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
    public async Task NonTransientError_IsNotRetried()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.BadRequest),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner);

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task TooManyRequests_IsNotHandledByTransientRetry()
    {
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = BuildClient(inner);

        using var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }
}

