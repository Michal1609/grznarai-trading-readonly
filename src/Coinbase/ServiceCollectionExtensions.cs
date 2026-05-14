using GrznarAi.Trading.ReadOnly.Coinbase.Authentication;
using GrznarAi.Trading.ReadOnly.Coinbase.Client;
using GrznarAi.Trading.ReadOnly.Coinbase.Configuration;
using GrznarAi.Trading.ReadOnly.Diagnostics;
using GrznarAi.Trading.ReadOnly.Http;
using GrznarAi.Trading.ReadOnly.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrznarAi.Trading.ReadOnly.Coinbase;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoinbaseClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = CoinbaseOptions.SectionName)
    {
        services.AddOptions<CoinbaseOptions>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.KeyName), $"{sectionName}:KeyName missing")
            .Validate(o => !string.IsNullOrWhiteSpace(o.PrivateKeyPem), $"{sectionName}:PrivateKeyPem missing")
            .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), $"{sectionName}:BaseUrl missing")
            .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out var u)
                           && u.Scheme == Uri.UriSchemeHttps
                           && (o.AllowCustomBaseAddress || u.Host == "api.coinbase.com"),
                "BaseUrl must be https://api.coinbase.com (or set AllowCustomBaseAddress=true).");

        services.AddSingleton<ApiDiagnostics>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<CoinbaseOptions>>().Value.Diagnostics;
            return new ApiDiagnostics(opt);
        });
        services.AddTransient<IApiDiagnostics>(sp => sp.GetRequiredService<ApiDiagnostics>());

        services.AddTransient<DiagnosticCapturingHandler>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<CoinbaseOptions>>().Value.Diagnostics;
            var diagnostics = sp.GetRequiredService<ApiDiagnostics>();
            return new DiagnosticCapturingHandler(diagnostics, opt, sp.GetService<ILogger<DiagnosticCapturingHandler>>());
        });

        services.AddSingleton<CdpJwtGenerator>();
        services.AddTransient<CdpJwtAuthHandler>();

        services.AddSingleton<IApiRateLimiter>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<CoinbaseOptions>>().Value.RateLimit;
            return new ApiRateLimiter(opt);
        });

        services.AddTransient<RateLimitHandler>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<CoinbaseOptions>>().Value.RateLimit;
            var rateLimiter = sp.GetRequiredService<IApiRateLimiter>();
            return new RateLimitHandler(opt, rateLimiter, sp.GetService<ILogger<RateLimitHandler>>());
        });

        services.AddTransient<TransientHttpErrorHandler>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<CoinbaseOptions>>().Value.Resilience;
            return new TransientHttpErrorHandler(opt, sp.GetService<ILogger<TransientHttpErrorHandler>>());
        });

        services.AddHttpClient<ICoinbaseClient, CoinbaseClient>((sp, http) =>
        {
            var opt = sp.GetRequiredService<IOptions<CoinbaseOptions>>().Value;
            http.BaseAddress = new Uri(opt.BaseUrl);
            http.Timeout = opt.Timeout;
            http.DefaultRequestHeaders.UserAgent.ParseAdd(opt.UserAgent);
            http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        })
        // Order: Diagnostic (outermost — captures final response after all retries)
        //     -> RateLimit (sees 429 before auth retries)
        //     -> Transient (5xx/network)
        //     -> Auth (innermost — re-issues JWT for each physical send)
        .AddHttpMessageHandler<DiagnosticCapturingHandler>()
        .AddHttpMessageHandler<RateLimitHandler>()
        .AddHttpMessageHandler<TransientHttpErrorHandler>()
        .AddHttpMessageHandler<CdpJwtAuthHandler>();

        // Forward domain interfaces to the same CoinbaseClient instance so callers can
        // inject a narrower surface (ICoinbaseAccountsClient / ICoinbasePortfoliosClient)
        // without resolving the full facade.
        services.AddTransient<ICoinbaseAccountsClient>(sp => sp.GetRequiredService<ICoinbaseClient>());
        services.AddTransient<ICoinbasePortfoliosClient>(sp => sp.GetRequiredService<ICoinbaseClient>());

        return services;
    }
}
