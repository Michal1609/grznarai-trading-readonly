using GrznarAi.Trading.ReadOnly.Diagnostics;
using GrznarAi.Trading.ReadOnly.Etoro.Configuration;
using GrznarAi.Trading.ReadOnly.Etoro.Services;
using GrznarAi.Trading.ReadOnly.Http;
using GrznarAi.Trading.ReadOnly.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrznarAi.Trading.ReadOnly.Etoro.Client;

public static class EToroClientExtensions
{
    private const string DefaultHost = "public-api.etoro.com";

    /// <summary>Registers eToro client services, binding options from <paramref name="configuration"/>.</summary>
    /// <param name="useKeyedRateLimiter">
    /// When <see langword="true"/>, registers <see cref="KeyedRateLimiter"/> which maintains
    /// independent rate limit windows per user key. Required when one application instance serves
    /// multiple eToro user keys simultaneously. Defaults to <see langword="false"/> (single-user).
    /// </param>
    public static IServiceCollection AddEToro(
        this IServiceCollection services, IConfiguration configuration,
        bool useKeyedRateLimiter = false)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddEToroOptions(options => options.Bind(configuration.GetSection("EToroOptions")));
        return services.AddEToroCore(useKeyedRateLimiter);
    }

    /// <summary>Registers eToro client services, configuring options via <paramref name="configureOptions"/>.</summary>
    /// <param name="useKeyedRateLimiter">
    /// When <see langword="true"/>, registers <see cref="KeyedRateLimiter"/> which maintains
    /// independent rate limit windows per user key. Required when one application instance serves
    /// multiple eToro user keys simultaneously. Defaults to <see langword="false"/> (single-user).
    /// </param>
    public static IServiceCollection AddEToro(
        this IServiceCollection services,
        Action<EToroOptions> configureOptions,
        bool useKeyedRateLimiter = false)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddEToroOptions(options => options.Configure(configureOptions));
        return services.AddEToroCore(useKeyedRateLimiter);
    }

    private static IServiceCollection AddEToroCore(this IServiceCollection services, bool useKeyedRateLimiter)
    {
        services.AddSingleton<ApiDiagnostics>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<EToroOptions>>().Value.Diagnostics;
            return new ApiDiagnostics(opt);
        });
        services.AddTransient<IApiDiagnostics>(sp => sp.GetRequiredService<ApiDiagnostics>());

        services.AddTransient<DiagnosticCapturingHandler>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<EToroOptions>>().Value.Diagnostics;
            var diagnostics = sp.GetRequiredService<ApiDiagnostics>();
            return new DiagnosticCapturingHandler(diagnostics, opt, sp.GetService<ILogger<DiagnosticCapturingHandler>>());
        });

        services.AddTransient<EToroAuthHandler>();

        services.AddTransient<TransientHttpErrorHandler>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<EToroOptions>>().Value.Resilience;
            return new TransientHttpErrorHandler(opt, sp.GetService<ILogger<TransientHttpErrorHandler>>());
        });

        if (useKeyedRateLimiter)
            services.AddSingleton<IApiRateLimiter>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<EToroOptions>>().Value.RateLimit;
                return new KeyedRateLimiter(opt, request =>
                {
                    if (request.Headers.TryGetValues("x-user-key", out var values))
                        return values.FirstOrDefault() ?? string.Empty;
                    return string.Empty;
                });
            });
        else
            services.AddSingleton<IApiRateLimiter>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<EToroOptions>>().Value.RateLimit;
                return new ApiRateLimiter(opt);
            });

        services.AddTransient<RateLimitHandler>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<EToroOptions>>().Value.RateLimit;
            var rateLimiter = sp.GetRequiredService<IApiRateLimiter>();
            return new RateLimitHandler(opt, rateLimiter, sp.GetService<ILogger<RateLimitHandler>>());
        });

        services.AddHttpClient<IEToroClient, EToroClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<EToroOptions>>().Value;
                client.BaseAddress = options.BaseAddress;
                client.Timeout = options.Timeout;

                if (!string.IsNullOrWhiteSpace(options.UserAgent))
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            })
            .AddHttpMessageHandler<DiagnosticCapturingHandler>()
            .AddHttpMessageHandler<EToroAuthHandler>()
            .AddHttpMessageHandler<TransientHttpErrorHandler>()
            .AddHttpMessageHandler<RateLimitHandler>();

        services.AddTransient<IEToroTradingClient>(sp => sp.GetRequiredService<IEToroClient>());
        services.AddTransient<IEToroMarketDataClient>(sp => sp.GetRequiredService<IEToroClient>());
        services.AddTransient<IEToroUserInfoClient>(sp => sp.GetRequiredService<IEToroClient>());
        services.AddTransient<IEToroSocialClient>(sp => sp.GetRequiredService<IEToroClient>());
        services.AddTransient<IEToroFeedClient>(sp => sp.GetRequiredService<IEToroClient>());
        services.AddTransient<IEToroWatchlistClient>(sp => sp.GetRequiredService<IEToroClient>());

        services.AddScoped<IEToroCalculationService, EToroCalculationService>();
        return services;
    }

    private static OptionsBuilder<EToroOptions> AddEToroOptions(
        this IServiceCollection services,
        Action<OptionsBuilder<EToroOptions>> configure)
    {
        var builder = services.AddOptions<EToroOptions>();
        configure(builder);

        return builder
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ApiKey),
                "EToroOptions.ApiKey must be configured.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.UserKey),
                "EToroOptions.UserKey must be configured.")
            .Validate(
                IsValidBaseAddress,
                "EToroOptions.BaseAddress must be an absolute HTTPS URI for public-api.etoro.com, must end with '/', and must not contain query, fragment, or user-info. Set AllowCustomBaseAddress to true to use a custom host.")
            .Validate(
                options => options.Timeout > TimeSpan.Zero && options.Timeout <= TimeSpan.FromMinutes(5),
                "EToroOptions.Timeout must be greater than zero and no more than five minutes.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.UserAgent),
                "EToroOptions.UserAgent must be configured.")
            .Validate(
                options => options.Environment.Equals("real", StringComparison.OrdinalIgnoreCase)
                           || options.Environment.Equals("demo", StringComparison.OrdinalIgnoreCase),
                "EToroOptions.Environment must be 'real' or 'demo'.")
            .Validate(
                options => options.RateLimit.PermitLimit > 0,
                "EToroOptions.RateLimit.PermitLimit must be greater than zero.")
            .Validate(
                options => options.RateLimit.Window > TimeSpan.Zero,
                "EToroOptions.RateLimit.Window must be greater than zero.")
            .Validate(
                options => options.RateLimit.MaxRetries >= 0,
                "EToroOptions.RateLimit.MaxRetries must be zero or greater.")
            .Validate(
                options => options.RateLimit.DefaultRetryDelay >= TimeSpan.Zero,
                "EToroOptions.RateLimit.DefaultRetryDelay must be zero or greater.")
            .Validate(
                options => options.RateLimit.MaxRetryDelay >= TimeSpan.Zero,
                "EToroOptions.RateLimit.MaxRetryDelay must be zero or greater.")
            .Validate(
                options => options.RateLimit.RetryJitterRatio >= 0 && options.RateLimit.RetryJitterRatio <= 1,
                "EToroOptions.RateLimit.RetryJitterRatio must be between zero and one.")
            .Validate(
                options => options.ErrorHandling is not null,
                "EToroOptions.ErrorHandling must be configured.")
            .Validate(
                options => options.ErrorHandling?.MaxResponseBodyLength is null or >= 0,
                "EToroOptions.ErrorHandling.MaxResponseBodyLength must be null or zero or greater.")
            .Validate(
                options => options.Resilience is not null,
                "EToroOptions.Resilience must be configured.")
            .Validate(
                options => options.Resilience?.MaxRetries >= 0,
                "EToroOptions.Resilience.MaxRetries must be zero or greater.")
            .Validate(
                options => options.Resilience?.DefaultRetryDelay >= TimeSpan.Zero,
                "EToroOptions.Resilience.DefaultRetryDelay must be zero or greater.")
            .Validate(
                options => options.Resilience?.MaxRetryDelay >= TimeSpan.Zero,
                "EToroOptions.Resilience.MaxRetryDelay must be zero or greater.")
            .Validate(
                options => options.Resilience?.RetryJitterRatio is >= 0 and <= 1,
                "EToroOptions.Resilience.RetryJitterRatio must be between zero and one.")
            .ValidateOnStart();
    }

    private static bool IsValidBaseAddress(EToroOptions options)
    {
        var baseAddress = options.BaseAddress;

        return baseAddress is not null
               && baseAddress.IsAbsoluteUri
               && baseAddress.Scheme == Uri.UriSchemeHttps
               && string.IsNullOrEmpty(baseAddress.Query)
               && string.IsNullOrEmpty(baseAddress.Fragment)
               && string.IsNullOrEmpty(baseAddress.UserInfo)
               && baseAddress.AbsolutePath.EndsWith('/')
               && (options.AllowCustomBaseAddress
                   || (baseAddress.Host.Equals(DefaultHost, StringComparison.OrdinalIgnoreCase)
                       && baseAddress.IsDefaultPort));
    }
}
