using System.Net;
using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Configuration;
using Microsoft.Extensions.Options;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class TransientHttpErrorHandlerTests
{
    private sealed class SequentialHandler(IReadOnlyList<Func<HttpResponseMessage>> factories) : HttpMessageHandler
    {
        private int _callCount;

        public int CallCount => _callCount;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var factory = factories[Math.Min(_callCount, factories.Count - 1)];
            _callCount++;
            return Task.FromResult(factory());
        }
    }

    private static HttpClient BuildClient(
        SequentialHandler inner,
        ResilienceOptions? resilienceOptions = null,
        Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        var options = Options.Create(new EToroOptions
        {
            Resilience = resilienceOptions ?? new ResilienceOptions
            {
                Enabled = true,
                MaxRetries = 2,
                DefaultRetryDelay = TimeSpan.FromMilliseconds(1),
                MaxRetryDelay = TimeSpan.FromMilliseconds(1),
                RetryJitterRatio = 0
            }
        });

        var handler = new TransientHttpErrorHandler(options, delay)
        {
            InnerHandler = inner
        };

        return new HttpClient(handler) { BaseAddress = new Uri("https://test.example.com/") };
    }

    [Test]
    public async Task TransientServerError_ThenSuccess_RetriesIdempotentRequest()
    {
        var inner = new SequentialHandler([
            () => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            () => new HttpResponseMessage(HttpStatusCode.OK)
        ]);
        var client = BuildClient(inner);

        var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(inner.CallCount, Is.EqualTo(2));
    }

    [Test]
    public async Task TransientServerError_PostDoesNotRetryByDefault()
    {
        var inner = new SequentialHandler([
            () => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            () => new HttpResponseMessage(HttpStatusCode.OK)
        ]);
        var client = BuildClient(inner);

        using var content = new StringContent("payload");
        var response = await client.PostAsync("/test", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task NonTransientError_IsNotRetried()
    {
        var inner = new SequentialHandler([
            () => new HttpResponseMessage(HttpStatusCode.BadRequest),
            () => new HttpResponseMessage(HttpStatusCode.OK)
        ]);
        var client = BuildClient(inner);

        var response = await client.GetAsync("/test");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(inner.CallCount, Is.EqualTo(1));
    }
}
