using System.Net.Http.Headers;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using Microsoft.Extensions.Options;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Authentication;

public sealed class CdpJwtAuthHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<CoinbaseOptions> _options;
    private readonly CdpJwtGenerator _generator;

    public CdpJwtAuthHandler(IOptionsMonitor<CoinbaseOptions> options, CdpJwtGenerator generator)
    {
        _options = options;
        _generator = generator;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var opt = _options.CurrentValue;
        var uri = request.RequestUri ?? throw new InvalidOperationException("Request URI is required");
        var host = uri.Host;
        var path = uri.AbsolutePath;
        var method = request.Method.Method;

        var token = _generator.Generate(opt.KeyName, opt.PrivateKeyPem, method, host, path, opt.JwtLifetimeSeconds);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
