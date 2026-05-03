using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace GrznarAi.Trading.ReadOnly.Tests.Client;

[TestFixture]
public class EToroClientExtensionsTests
{
    [Test]
    public void AddEToro_ActionOverload_ConfiguresOptions()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "api-key";
            options.UserKey = "user-key";
            options.BaseAddress = new Uri("https://example.com/api/");
            options.AllowCustomBaseAddress = true;
            options.Timeout = TimeSpan.FromSeconds(30);
            options.UserAgent = "GrznarAi.Trading.ReadOnly.Tests";
        });

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<EToroOptions>>().Value;

        Assert.That(options.ApiKey, Is.EqualTo("api-key"));
        Assert.That(options.UserKey, Is.EqualTo("user-key"));
        Assert.That(options.BaseAddress, Is.EqualTo(new Uri("https://example.com/api/")));
        Assert.That(options.Timeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
        Assert.That(options.UserAgent, Is.EqualTo("GrznarAi.Trading.ReadOnly.Tests"));
    }

    [Test]
    public void AddEToro_CanResolveClientWithRateLimitPipeline()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "api-key";
            options.UserKey = "user-key";
            options.BaseAddress = new Uri("https://example.com/api/");
            options.AllowCustomBaseAddress = true;
            options.UserAgent = "GrznarAi.Trading.ReadOnly.Tests";
            options.RateLimit.PermitLimit = 60;
        });

        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IEToroClient>();

        Assert.That(client, Is.Not.Null);
    }

    [Test]
    public void AddEToro_RegistersDomainSpecificClientInterfaces()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "api-key";
            options.UserKey = "user-key";
            options.BaseAddress = new Uri("https://example.com/api/");
            options.AllowCustomBaseAddress = true;
            options.UserAgent = "GrznarAi.Trading.ReadOnly.Tests";
        });

        using var provider = services.BuildServiceProvider();

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IEToroTradingClient>(), Is.Not.Null);
            Assert.That(provider.GetRequiredService<IEToroMarketDataClient>(), Is.Not.Null);
            Assert.That(provider.GetRequiredService<IEToroUserInfoClient>(), Is.Not.Null);
            Assert.That(provider.GetRequiredService<IEToroSocialClient>(), Is.Not.Null);
            Assert.That(provider.GetRequiredService<IEToroFeedClient>(), Is.Not.Null);
            Assert.That(provider.GetRequiredService<IEToroWatchlistClient>(), Is.Not.Null);
        });
    }

    [Test]
    public void AddEToro_NullConfiguration_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        IConfiguration nullConfig = null!;
        Assert.Throws<ArgumentNullException>(() => services.AddEToro(nullConfig));
    }

    [Test]
    public void AddEToro_InvalidOptions_ThrowsOptionsValidationExceptionOnStartupValidation()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "";
            options.UserKey = "";
            options.BaseAddress = new Uri("http://example.com/api/");
            options.Timeout = TimeSpan.Zero;
        });

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());

        Assert.That(exception!.Failures, Has.Some.Contains("ApiKey"));
        Assert.That(exception.Failures, Has.Some.Contains("UserKey"));
        Assert.That(exception.Failures, Has.Some.Contains("BaseAddress"));
        Assert.That(exception.Failures, Has.Some.Contains("Timeout"));
    }

    [Test]
    public void AddEToro_InvalidOptions_ThrowsOptionsValidationException()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "";
            options.UserKey = "";
            options.BaseAddress = new Uri("http://example.com/api/");
            options.Timeout = TimeSpan.Zero;
        });

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => _ = provider.GetRequiredService<IOptions<EToroOptions>>().Value);

        Assert.That(exception!.Failures, Has.Some.Contains("ApiKey"));
        Assert.That(exception.Failures, Has.Some.Contains("UserKey"));
        Assert.That(exception.Failures, Has.Some.Contains("BaseAddress"));
        Assert.That(exception.Failures, Has.Some.Contains("Timeout"));
    }

    [Test]
    public void AddEToro_InvalidRateLimitOptions_ThrowsOptionsValidationException()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "api-key";
            options.UserKey = "user-key";
            options.BaseAddress = new Uri("https://example.com/api/");
            options.AllowCustomBaseAddress = true;
            options.UserAgent = "GrznarAi.Trading.ReadOnly.Tests";
            options.RateLimit.PermitLimit = 0;
            options.RateLimit.Window = TimeSpan.Zero;
            options.RateLimit.MaxRetries = -1;
            options.RateLimit.RetryJitterRatio = 2;
        });

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => _ = provider.GetRequiredService<IOptions<EToroOptions>>().Value);

        Assert.That(exception!.Failures, Has.Some.Contains("RateLimit.PermitLimit"));
        Assert.That(exception.Failures, Has.Some.Contains("RateLimit.Window"));
        Assert.That(exception.Failures, Has.Some.Contains("RateLimit.MaxRetries"));
        Assert.That(exception.Failures, Has.Some.Contains("RateLimit.RetryJitterRatio"));
    }

    [Test]
    public void AddEToro_InvalidErrorHandlingOptions_ThrowsOptionsValidationException()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "api-key";
            options.UserKey = "user-key";
            options.BaseAddress = new Uri("https://example.com/api/");
            options.AllowCustomBaseAddress = true;
            options.UserAgent = "GrznarAi.Trading.ReadOnly.Tests";
            options.ErrorHandling.MaxResponseBodyLength = -1;
        });

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => _ = provider.GetRequiredService<IOptions<EToroOptions>>().Value);

        Assert.That(exception!.Failures, Has.Some.Contains("ErrorHandling.MaxResponseBodyLength"));
    }

    [Test]
    public void AddEToro_InvalidResilienceOptions_ThrowsOptionsValidationException()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "api-key";
            options.UserKey = "user-key";
            options.BaseAddress = new Uri("https://example.com/api/");
            options.AllowCustomBaseAddress = true;
            options.UserAgent = "GrznarAi.Trading.ReadOnly.Tests";
            options.Resilience.MaxRetries = -1;
            options.Resilience.DefaultRetryDelay = TimeSpan.FromMilliseconds(-1);
            options.Resilience.RetryJitterRatio = 2;
        });

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => _ = provider.GetRequiredService<IOptions<EToroOptions>>().Value);

        Assert.That(exception!.Failures, Has.Some.Contains("Resilience.MaxRetries"));
        Assert.That(exception.Failures, Has.Some.Contains("Resilience.DefaultRetryDelay"));
        Assert.That(exception.Failures, Has.Some.Contains("Resilience.RetryJitterRatio"));
    }

    [Test]
    public void AddEToro_InvalidUserAgent_ThrowsWhenClientIsResolved()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "api-key";
            options.UserKey = "user-key";
            options.BaseAddress = new Uri("https://public-api.etoro.com/api/v1/");
            options.UserAgent = "bad\r\nagent";
        });

        using var provider = services.BuildServiceProvider();

        Assert.Throws<FormatException>(() => provider.GetRequiredService<IEToroClient>());
    }

    [Test]
    public void AddEToro_CustomBaseAddressWithoutOptIn_ThrowsOptionsValidationException()
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "api-key";
            options.UserKey = "user-key";
            options.BaseAddress = new Uri("https://example.com/api/");
            options.UserAgent = "GrznarAi.Trading.ReadOnly.Tests";
        });

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => _ = provider.GetRequiredService<IOptions<EToroOptions>>().Value);

        Assert.That(exception!.Failures, Has.Some.Contains("BaseAddress"));
    }

    [TestCase("https://public-api.etoro.com/api/v1?apiKey=secret")]
    [TestCase("https://public-api.etoro.com/api/v1/#fragment")]
    [TestCase("https://user:password@public-api.etoro.com/api/v1/")]
    [TestCase("https://public-api.etoro.com:444/api/v1/")]
    [TestCase("https://public-api.etoro.com/api/v1")]
    public void AddEToro_InvalidBaseAddressShape_ThrowsOptionsValidationException(string baseAddress)
    {
        var services = new ServiceCollection();

        services.AddEToro(options =>
        {
            options.ApiKey = "api-key";
            options.UserKey = "user-key";
            options.BaseAddress = new Uri(baseAddress);
            options.UserAgent = "GrznarAi.Trading.ReadOnly.Tests";
        });

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => _ = provider.GetRequiredService<IOptions<EToroOptions>>().Value);

        Assert.That(exception!.Failures, Has.Some.Contains("BaseAddress"));
    }
}
