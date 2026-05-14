using GrznarAi.Trading.ReadOnly.Etoro.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["EToroOptions:ApiKey"] = "aot-smoke-api-key",
        ["EToroOptions:UserKey"] = "aot-smoke-user-key",
        ["EToroOptions:Environment"] = "demo",
        ["EToroOptions:BaseAddress"] = "https://public-api.etoro.com/api/v1/",
        ["EToroOptions:AllowCustomBaseAddress"] = "false",
        ["EToroOptions:Timeout"] = "00:01:40",
        ["EToroOptions:UserAgent"] = "GrznarAi.Trading.ReadOnly.Etoro.Aot.SmokeTest",
        ["EToroOptions:RateLimit:Enabled"] = "true",
        ["EToroOptions:RateLimit:PermitLimit"] = "60",
        ["EToroOptions:RateLimit:Window"] = "00:01:00",
        ["EToroOptions:RateLimit:MaxRetries"] = "0",
        ["EToroOptions:RateLimit:DefaultRetryDelay"] = "00:01:00",
        ["EToroOptions:RateLimit:MaxRetryDelay"] = "00:01:00",
        ["EToroOptions:RateLimit:RetryJitterRatio"] = "0.1",
        ["EToroOptions:RateLimit:RetryNonIdempotentRequests"] = "false",
        ["EToroOptions:ErrorHandling:IncludeResponseBody"] = "true",
        ["EToroOptions:ErrorHandling:RedactResponseBody"] = "true",
        ["EToroOptions:ErrorHandling:MaxResponseBodyLength"] = "4096",
        ["EToroOptions:Resilience:Enabled"] = "true",
        ["EToroOptions:Resilience:MaxRetries"] = "2",
        ["EToroOptions:Resilience:DefaultRetryDelay"] = "00:00:01",
        ["EToroOptions:Resilience:MaxRetryDelay"] = "00:00:10",
        ["EToroOptions:Resilience:RetryJitterRatio"] = "0.1",
        ["EToroOptions:Resilience:RetryNonIdempotentRequests"] = "false"
    })
    .Build();

var services = new ServiceCollection();
services.AddEToro(configuration);

await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateOnBuild = true,
    ValidateScopes = true
});

_ = provider.GetRequiredService<IEToroClient>();
Console.WriteLine("EToro AOT smoke test initialized.");
