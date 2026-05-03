using System.Net;
using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class EToroAuthHandlerTests
{
    [Test]
    public async Task SendAsync_WithValidCredentials_AddsAuthHeadersAndRequestId()
    {
        var capture = new HeaderCaptureHandler();
        using var client = CreateHttpClient(capture, apiKey: "api-key", userKey: "user-key");

        using var response = await client.GetAsync("https://example.test/ping");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(capture.ApiKey, Is.EqualTo("api-key"));
        Assert.That(capture.UserKey, Is.EqualTo("user-key"));
        Assert.That(capture.RequestId, Is.Not.Null.And.Not.Empty);
    }

    [TestCase("api\rkey")]
    [TestCase("api\nkey")]
    [TestCase("api\0key")]
    [TestCase("api\u001fkey")]
    [TestCase("api\u007fkey")]
    public void SendAsync_WithInvalidApiKey_ThrowsInvalidOperationException(string apiKey)
    {
        using var client = CreateHttpClient(new HeaderCaptureHandler(), apiKey, userKey: "user-key");

        Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAsync("https://example.test/ping"));
    }

    [TestCase("user\rkey")]
    [TestCase("user\nkey")]
    [TestCase("user\0key")]
    [TestCase("user\u001fkey")]
    [TestCase("user\u007fkey")]
    public void SendAsync_WithInvalidUserKey_ThrowsInvalidOperationException(string userKey)
    {
        using var client = CreateHttpClient(new HeaderCaptureHandler(), apiKey: "api-key", userKey);

        Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAsync("https://example.test/ping"));
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler innerHandler, string apiKey, string userKey)
    {
        var handler = new EToroAuthHandler(Options.Create(new EToroOptions
        {
            ApiKey = apiKey,
            UserKey = userKey
        }))
        {
            InnerHandler = innerHandler
        };

        return new HttpClient(handler);
    }

    private sealed class HeaderCaptureHandler : HttpMessageHandler
    {
        public string? ApiKey { get; private set; }
        public string? UserKey { get; private set; }
        public string? RequestId { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            ApiKey = request.Headers.GetValues("x-api-key").SingleOrDefault();
            UserKey = request.Headers.GetValues("x-user-key").SingleOrDefault();
            RequestId = request.Headers.GetValues("x-request-id").SingleOrDefault();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            return Task.FromResult(response);
        }
    }
}
