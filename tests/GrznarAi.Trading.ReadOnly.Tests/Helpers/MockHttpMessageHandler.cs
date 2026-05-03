using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace GrznarAi.Trading.ReadOnly.Tests.Helpers;

public sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _responseJson;
    private readonly HttpStatusCode _statusCode;

    public string? LastRequestUri { get; private set; }
    public HttpRequestHeaders? LastRequestHeaders { get; private set; }

    public MockHttpMessageHandler(string responseJson, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseJson = responseJson;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri?.ToString();
        LastRequestHeaders = request.Headers;

        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseJson, Encoding.UTF8, "application/json"),
            RequestMessage = request
        });
    }
}
