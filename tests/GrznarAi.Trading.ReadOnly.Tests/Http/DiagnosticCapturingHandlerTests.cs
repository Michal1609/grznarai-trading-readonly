using System.Net;
using System.Net.Http.Headers;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Diagnostics;
using GrznarAi.Trading.ReadOnly.Http;
using GrznarAi.Trading.ReadOnly.Tests.Helpers;

namespace GrznarAi.Trading.ReadOnly.Tests.Http;

[TestFixture]
public class DiagnosticCapturingHandlerTests
{
    private static HttpClient BuildClient(
        SequentialHttpMessageHandler inner,
        DiagnosticOptions options,
        ApiDiagnostics diagnostics)
    {
        var handler = new DiagnosticCapturingHandler(diagnostics, options)
        {
            InnerHandler = inner
        };

        return new HttpClient(handler) { BaseAddress = new Uri("https://test.example.com/") };
    }

    [Test]
    public async Task SendAsync_SuccessfulResponse_StoresSnapshotAndKeepsBodyReadable()
    {
        var options = new DiagnosticOptions { Enabled = true };
        var diagnostics = new ApiDiagnostics(options);
        var inner = new SequentialHttpMessageHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            };
            response.Headers.TryAddWithoutValidation("Sunset", "Tue, 12 May 2026 10:00:00 GMT");
            return response;
        });
        using var client = BuildClient(inner, options, diagnostics);

        using var response = await client.GetAsync("/portfolio?limit=1");

        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("{\"ok\":true}"));
        Assert.That(diagnostics.Last, Is.Not.Null);
        Assert.That(diagnostics.Last!.Method, Is.EqualTo("GET"));
        Assert.That(diagnostics.Last.RequestUri?.PathAndQuery, Is.EqualTo("/portfolio?limit=1"));
        Assert.That(diagnostics.Last.StatusCode, Is.EqualTo(200));
        Assert.That(diagnostics.Last.ResponseBody, Is.EqualTo("{\"ok\":true}"));
        Assert.That(
            diagnostics.Last.ResponseHeaders?["Sunset"],
            Is.EqualTo(new[] { "Tue, 12 May 2026 10:00:00 GMT" }));
    }

    [Test]
    public async Task SendAsync_HttpError_StoresSnapshot()
    {
        var options = new DiagnosticOptions { Enabled = true };
        var diagnostics = new ApiDiagnostics(options);
        var inner = new SequentialHttpMessageHandler(() => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            ReasonPhrase = "Bad Request",
            Content = new StringContent("{\"error\":\"invalid\"}")
        });
        using var client = BuildClient(inner, options, diagnostics);

        using var response = await client.GetAsync("/broken");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(diagnostics.Last, Is.Not.Null);
        Assert.That(diagnostics.Last!.StatusCode, Is.EqualTo(400));
        Assert.That(diagnostics.Last.ReasonPhrase, Is.EqualTo("Bad Request"));
        Assert.That(diagnostics.Last.ResponseBody, Is.EqualTo("{\"error\":\"invalid\"}"));
    }

    [Test]
    public async Task SendAsync_ResponseBodyLongerThanLimit_TruncatesCapturedBodyOnly()
    {
        var options = new DiagnosticOptions
        {
            Enabled = true,
            MaxBodyBytes = 4
        };
        var diagnostics = new ApiDiagnostics(options);
        var inner = new SequentialHttpMessageHandler(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("abcdef")
        });
        using var client = BuildClient(inner, options, diagnostics);

        using var response = await client.GetAsync("/body");

        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("abcdef"));
        Assert.That(diagnostics.Last?.ResponseBody, Is.EqualTo("abcd"));
        Assert.That(diagnostics.Last?.ResponseBodyTruncated, Is.True);
    }

    [Test]
    public async Task SendAsync_RedactedHeaders_DoNotExposeSensitiveValues()
    {
        var options = new DiagnosticOptions
        {
            Enabled = true,
            CaptureRequestHeaders = true
        };
        var diagnostics = new ApiDiagnostics(options);
        var inner = new SequentialHttpMessageHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            };
            response.Headers.TryAddWithoutValidation("Set-Cookie", "session=secret");
            response.Headers.TryAddWithoutValidation("X-Trace-Id", "trace-123");
            return response;
        });
        using var client = BuildClient(inner, options, diagnostics);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/headers");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "secret");
        request.Headers.TryAddWithoutValidation("X-API-Key", "api-secret");
        request.Headers.TryAddWithoutValidation("X-Client", "test-client");

        using var response = await client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(diagnostics.Last?.RequestHeaders?["Authorization"], Is.EqualTo(new[] { "[REDACTED]" }));
        Assert.That(diagnostics.Last?.RequestHeaders?["X-API-Key"], Is.EqualTo(new[] { "[REDACTED]" }));
        Assert.That(diagnostics.Last?.RequestHeaders?["X-Client"], Is.EqualTo(new[] { "test-client" }));
        Assert.That(diagnostics.Last?.ResponseHeaders?["Set-Cookie"], Is.EqualTo(new[] { "[REDACTED]" }));
        Assert.That(diagnostics.Last?.ResponseHeaders?["X-Trace-Id"], Is.EqualTo(new[] { "trace-123" }));
    }

    [Test]
    public async Task SendAsync_HistorySizeLimit_KeepsLatestSnapshotsNewestFirst()
    {
        var options = new DiagnosticOptions
        {
            Enabled = true,
            HistorySize = 2
        };
        var diagnostics = new ApiDiagnostics(options);
        var inner = new SequentialHttpMessageHandler(
            () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("first") },
            () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("second") },
            () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("third") });
        using var client = BuildClient(inner, options, diagnostics);

        using var first = await client.GetAsync("/one");
        using var second = await client.GetAsync("/two");
        using var third = await client.GetAsync("/three");

        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(third.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(diagnostics.Last?.ResponseBody, Is.EqualTo("third"));
        Assert.That(diagnostics.History.Select(snapshot => snapshot.ResponseBody), Is.EqualTo(new[] { "third", "second" }));
    }
}
