using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GrznarAi.Trading.ReadOnly.Configuration;
using GrznarAi.Trading.ReadOnly.Diagnostics;
using GrznarAi.Trading.ReadOnly.Http;
using GrznarAi.Trading.ReadOnly.Json;
using GrznarAi.Trading.ReadOnly.Querying;
using GrznarAi.Trading.ReadOnly.RateLimiting;

var query = new QueryStringBuilder()
    .Add("symbol", "BTC USD")
    .Add("limit", 10)
    .AddIfHasValue("active", true)
    .ToString();

if (query != "?symbol=BTC%20USD&limit=10&active=true")
    return Fail($"Unexpected query string: {query}");

var diagnosticOptions = new DiagnosticOptions
{
    Enabled = true,
    CaptureRequestHeaders = true,
    MaxBodyBytes = 4,
    HistorySize = 2
};
var diagnostics = new ApiDiagnostics(diagnosticOptions);
var rateLimitOptions = new RateLimitOptions
{
    Enabled = false,
    MaxRetries = 0,
    RetryJitterRatio = 0
};

using var client = new HttpClient(new DiagnosticCapturingHandler(diagnostics, diagnosticOptions)
{
    InnerHandler = new TransientHttpErrorHandler(new ResilienceOptions { Enabled = false })
    {
        InnerHandler = new RateLimitHandler(rateLimitOptions, new ApiRateLimiter(rateLimitOptions))
        {
            InnerHandler = new StaticResponseHandler()
        }
    }
})
{
    BaseAddress = new Uri("https://core-aot.example/")
};

using var request = new HttpRequestMessage(HttpMethod.Get, "/prices" + query);
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "secret");
request.Headers.TryAddWithoutValidation("X-Client", "aot-smoke");

using var response = await client.SendAsync(request);
var body = await response.Content.ReadAsStringAsync();

if (response.StatusCode != HttpStatusCode.OK || body != "abcdef")
    return Fail($"Unexpected HTTP response: {(int)response.StatusCode} {body}");

var snapshot = diagnostics.Last;
if (snapshot is null)
    return Fail("Diagnostic snapshot was not captured.");

if (snapshot.StatusCode != 200
    || snapshot.ResponseBody != "abcd"
    || !snapshot.ResponseBodyTruncated
    || snapshot.RequestHeaders?["Authorization"][0] != "[REDACTED]"
    || snapshot.RequestHeaders?["X-Client"][0] != "aot-smoke"
    || snapshot.ResponseHeaders?["Set-Cookie"][0] != "[REDACTED]")
{
    return Fail("Diagnostic snapshot did not match expected values.");
}

if (diagnostics.History.Count != 1 || diagnostics.History[0] != snapshot)
    return Fail("Diagnostic history did not retain the latest snapshot.");

var converter = new DecimalStringConverter();
var reader = new Utf8JsonReader("\"12.34\""u8);
reader.Read();
var value = converter.Read(ref reader, typeof(decimal), new JsonSerializerOptions());
if (value != 12.34m)
    return Fail($"Unexpected decimal conversion value: {value}");

using var stream = new MemoryStream();
using (var writer = new Utf8JsonWriter(stream))
{
    converter.Write(writer, value, new JsonSerializerOptions());
}

if (Encoding.UTF8.GetString(stream.ToArray()) != "\"12.34\"")
    return Fail("Decimal converter wrote unexpected JSON.");

Console.WriteLine("Core AOT smoke OK.");
return 0;

static int Fail(string message)
{
    Console.Error.WriteLine("Core AOT smoke FAILED: " + message);
    return 1;
}

internal sealed class StaticResponseHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("abcdef", Encoding.UTF8, "text/plain")
        };
        response.Headers.TryAddWithoutValidation("Set-Cookie", "session=secret");
        response.Headers.TryAddWithoutValidation("Sunset", "Tue, 12 May 2026 10:00:00 GMT");

        return Task.FromResult(response);
    }
}

