/*
 * Demo 02 - Portfolio Concentration Analyzer
 * ------------------------------------------
 * Fetches the current eToro PnL snapshot and calculates portfolio
 * concentration metrics: Top 1 / Top 3 / Top 5, HHI, and a simple
 * diversification interpretation.
 *
 * Usage:
 *   dotnet run --project examples/Demo02.PortfolioConcentrationAnalyzer -- --account real
 *   dotnet run --project examples/Demo02.PortfolioConcentrationAnalyzer -- --account real --include-cash false
 *
 * Credentials - choose one:
 *   A) User Secrets, recommended:
 *        This demo uses the same UserSecretsId as the integration tests:
 *        GrznarAi.Trading.ReadOnly.Tests.Integration
 *
 *        If you already configured secrets for the test project in Visual Studio,
 *        this demo will read the same values.
 *
 *        To set them from the CLI:
 *        dotnet user-secrets set "EToroOptions:ApiKey"  "your-api-key"  --project tests/GrznarAi.Trading.ReadOnly.Tests
 *        dotnet user-secrets set "EToroOptions:UserKey" "your-user-key" --project tests/GrznarAi.Trading.ReadOnly.Tests
 *   B) Environment variables:
 *        EToroOptions__ApiKey=...  EToroOptions__UserKey=...
 *   C) appsettings.json with real values, only for local throwaway demos.
 *
 * The sample defaults to the real PnL endpoint because the public demo PnL
 * endpoint may reject otherwise valid credentials. Passing --account demo
 * is supported, but it depends on eToro account/API access.
 */

using System.Globalization;
using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Models.Pnl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var options = ParseArgs(args);
if (!options.IsValid)
{
    Console.Error.WriteLine(options.ErrorMessage);
    Console.Error.WriteLine();
    PrintUsage();
    return 2;
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var credentialProblem = ValidateCredentials(configuration);
if (credentialProblem is not null)
{
    Console.Error.WriteLine(credentialProblem);
    Console.Error.WriteLine();
    Console.Error.WriteLine("Set credentials with user secrets or environment variables before running the demo.");
    return 2;
}

var services = new ServiceCollection();
services.AddEToro(configuration);

await using var serviceProvider = services.BuildServiceProvider(
    new ServiceProviderOptions { ValidateOnBuild = true });

var trading = serviceProvider.GetRequiredService<IEToroTradingClient>();
var account = options.Account;

PnlResponse pnl;
try
{
    pnl = await trading.GetPnlAsync(account);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error fetching {FormatAccount(account)} PnL: {ex.Message}");
    return 1;
}

var analysis = AnalyzePortfolio(pnl, options.IncludeCash);
PrintReport(analysis, account, options.IncludeCash);

return analysis.CanCalculate ? 0 : 1;

static CliOptions ParseArgs(string[] args)
{
    var account = EToroEnvironment.Real;
    var includeCash = true;

    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        if (arg.Equals("--account", StringComparison.OrdinalIgnoreCase))
        {
            if (++i >= args.Length)
                return CliOptions.Invalid("Missing value for --account. Expected demo or real.");

            if (args[i].Equals("real", StringComparison.OrdinalIgnoreCase))
                account = EToroEnvironment.Real;
            else if (args[i].Equals("demo", StringComparison.OrdinalIgnoreCase))
                account = EToroEnvironment.Demo;
            else
                return CliOptions.Invalid($"Invalid --account value '{args[i]}'. Expected demo or real.");
        }
        else if (arg.Equals("--include-cash", StringComparison.OrdinalIgnoreCase))
        {
            if (++i >= args.Length)
                return CliOptions.Invalid("Missing value for --include-cash. Expected true or false.");

            if (!bool.TryParse(args[i], out includeCash))
                return CliOptions.Invalid($"Invalid --include-cash value '{args[i]}'. Expected true or false.");
        }
        else if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                 arg.Equals("-h", StringComparison.OrdinalIgnoreCase))
        {
            PrintUsage();
            Environment.Exit(0);
        }
        else
        {
            return CliOptions.Invalid($"Unknown argument '{arg}'.");
        }
    }

    return new CliOptions(account, includeCash);
}

static PortfolioAnalysis AnalyzePortfolio(PnlResponse pnl, bool includeCash)
{
    var warnings = new List<string>();

    var manualOrdersAmount = pnl.OrdersForOpen.Where(o => o.MirrorId == 0).Sum(o => o.Amount);
    var manualExternalCosts = pnl.OrdersForOpen.Where(o => o.MirrorId == 0).Sum(o => o.TotalExternalCosts);
    var mitOrdersAmount = pnl.Orders.Sum(o => o.Amount);

    var availableCash = pnl.Credit - manualOrdersAmount - mitOrdersAmount;

    var totalInvested =
        pnl.Positions.Sum(p => p.Amount)
        + pnl.MirrorPortfolios.Sum(m => m.Positions.Sum(p => p.Amount))
        + pnl.MirrorPortfolios.Sum(m => m.AvailableAmount - m.ClosedPositionsNetProfit)
        + manualOrdersAmount
        + mitOrdersAmount
        + manualExternalCosts;

    var unrealizedPnl =
        pnl.Positions.Sum(p => p.PnL)
        + pnl.MirrorPortfolios.Sum(m => m.Positions.Sum(p => p.PnL))
        + pnl.MirrorPortfolios.Sum(m => m.ClosedPositionsNetProfit);

    var equity = availableCash + totalInvested + unrealizedPnl;
    if (equity <= 0m)
    {
        return new PortfolioAnalysis(
            equity,
            [],
            Top1: 0m,
            Top3: 0m,
            Top5: 0m,
            Hhi: 0m,
            Result: "Cannot calculate concentration because equity is zero or negative.",
            Warnings: warnings,
            CanCalculate: false);
    }

    var bucketValues = new Dictionary<string, BucketDraft>(StringComparer.OrdinalIgnoreCase);

    foreach (var position in pnl.Positions)
        AddPosition(bucketValues, position.InstrumentId, position.Amount + position.PnL, warnings);

    foreach (var position in pnl.MirrorPortfolios.SelectMany(m => m.Positions))
        AddPosition(bucketValues, position.InstrumentId, position.Amount + position.PnL, warnings);

    var buckets = bucketValues.Values
        .Select(b => new PortfolioBucket(b.Key, b.DisplayName, b.CurrentValue, b.CurrentValue / equity))
        .ToList();

    if (includeCash)
    {
        if (availableCash >= 0m)
        {
            buckets.Add(new PortfolioBucket("CASH", "Cash", availableCash, availableCash / equity));
        }
        else
        {
            warnings.Add("Available cash is negative; the cash bucket was excluded from HHI.");
        }
    }

    buckets = buckets
        .OrderByDescending(b => b.Weight)
        .ThenBy(b => b.Key, StringComparer.OrdinalIgnoreCase)
        .ToList();

    var hhiBuckets = buckets
        .Where(b => !b.Key.Equals("CASH", StringComparison.OrdinalIgnoreCase) || availableCash >= 0m)
        .ToList();

    var top1 = SumTopWeights(buckets, 1);
    var top3 = SumTopWeights(buckets, 3);
    var top5 = SumTopWeights(buckets, 5);
    var hhi = hhiBuckets.Sum(b => b.Weight * b.Weight);

    var result = Interpret(hhi);
    if (buckets.Count == 1 && buckets[0].Key.Equals("CASH", StringComparison.OrdinalIgnoreCase))
        result = "Fully concentrated in cash";

    if (top1 >= 0.40m)
        warnings.Add("Single-position concentration risk: the largest bucket is at least 40% of equity.");
    if (top3 >= 0.70m)
        warnings.Add("Top-3 concentration risk: the top three buckets are at least 70% of equity.");
    if (buckets.Count < 5)
        warnings.Add("Low number of holdings: fewer than five buckets are included.");

    return new PortfolioAnalysis(equity, buckets, top1, top3, top5, hhi, result, warnings, CanCalculate: true);
}

static void AddPosition(
    IDictionary<string, BucketDraft> buckets,
    int instrumentId,
    decimal currentValue,
    ICollection<string> warnings)
{
    var key = instrumentId > 0 ? instrumentId.ToString(CultureInfo.InvariantCulture) : "UNKNOWN";
    var displayName = instrumentId > 0 ? $"Instrument {instrumentId}" : "Unknown instrument";

    if (currentValue < 0m)
    {
        warnings.Add($"{displayName} has a negative current value ({Usd(currentValue)}); it was set to $0.00.");
        currentValue = 0m;
    }

    if (buckets.TryGetValue(key, out var existing))
    {
        buckets[key] = existing with { CurrentValue = existing.CurrentValue + currentValue };
    }
    else
    {
        buckets[key] = new BucketDraft(key, displayName, currentValue);
    }
}

static void PrintReport(PortfolioAnalysis analysis, EToroEnvironment account, bool includeCash)
{
    Console.WriteLine("Portfolio Concentration Analyzer");
    Console.WriteLine($"Account: {FormatAccount(account)}");
    Console.WriteLine($"Include cash: {includeCash.ToString().ToLowerInvariant()}");
    Console.WriteLine();

    if (!analysis.CanCalculate)
    {
        Console.WriteLine(analysis.Result);
        return;
    }

    Console.WriteLine($"Equity: {Usd(analysis.Equity)}");
    Console.WriteLine($"Buckets: {analysis.Buckets.Count}");
    Console.WriteLine();

    if (analysis.Buckets.Count > 0)
    {
        Console.WriteLine($"{"Rank",-5} {"Symbol",-24} {"Value",14} {"Weight",10}");
        for (var i = 0; i < Math.Min(10, analysis.Buckets.Count); i++)
        {
            var bucket = analysis.Buckets[i];
            Console.WriteLine(
                $"{i + 1,-5} {Shorten(bucket.DisplayName, 24),-24} {Usd(bucket.CurrentValue),14} {Percent(bucket.Weight),10}");
        }
    }
    else
    {
        Console.WriteLine("No positions or cash buckets were included.");
    }

    Console.WriteLine();
    Console.WriteLine($"Top 1 concentration: {Percent(analysis.Top1)}");
    Console.WriteLine($"Top 3 concentration: {Percent(analysis.Top3)}");
    Console.WriteLine($"Top 5 concentration: {Percent(analysis.Top5)}");
    Console.WriteLine($"HHI:                 {analysis.Hhi:F4}");
    Console.WriteLine();
    Console.WriteLine($"Result: {analysis.Result}");

    if (analysis.Warnings.Count > 0)
    {
        Console.WriteLine("Warnings:");
        foreach (var warning in analysis.Warnings.Distinct(StringComparer.OrdinalIgnoreCase))
            Console.WriteLine($"- {warning}");
    }
}

static void PrintUsage()
{
    Console.WriteLine("Portfolio Concentration Analyzer");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project examples/Demo02.PortfolioConcentrationAnalyzer -- --account real --include-cash true");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --account demo|real        Account endpoint to query. Default: real.");
    Console.WriteLine("  --include-cash true|false  Include cash as a concentration bucket. Default: true.");
}

static string? ValidateCredentials(IConfiguration configuration)
{
    var apiKey = configuration["EToroOptions:ApiKey"];
    var userKey = configuration["EToroOptions:UserKey"];

    if (string.IsNullOrWhiteSpace(apiKey) || IsPlaceholder(apiKey))
        return "Missing EToroOptions:ApiKey.";
    if (string.IsNullOrWhiteSpace(userKey) || IsPlaceholder(userKey))
        return "Missing EToroOptions:UserKey.";

    return null;
}

static bool IsPlaceholder(string value)
{
    return value.Contains("replace-me", StringComparison.OrdinalIgnoreCase)
           || value.Contains("dummy", StringComparison.OrdinalIgnoreCase)
           || value.Contains("your-", StringComparison.OrdinalIgnoreCase);
}

static decimal SumTopWeights(IReadOnlyList<PortfolioBucket> buckets, int count)
{
    return buckets.Take(count).Sum(b => b.Weight);
}

static string Interpret(decimal hhi)
{
    if (hhi < 0.10m)
        return "Well diversified";
    if (hhi < 0.20m)
        return "Moderately concentrated";

    return "Highly concentrated";
}

static string FormatAccount(EToroEnvironment account)
{
    return account == EToroEnvironment.Demo ? "demo" : "real";
}

static string Usd(decimal value)
{
    var culture = CultureInfo.GetCultureInfo("en-US");
    return value.ToString("C", culture);
}

static string Percent(decimal value)
{
    return (value * 100m).ToString("F2", CultureInfo.InvariantCulture) + "%";
}

static string Shorten(string value, int maxLength)
{
    if (value.Length <= maxLength)
        return value;

    return value[..Math.Max(0, maxLength - 3)] + "...";
}

#pragma warning disable CA1050 // Demo keeps the requested top-level record shape.
public sealed record PortfolioBucket(
    string Key,
    string DisplayName,
    decimal CurrentValue,
    decimal Weight);
#pragma warning restore CA1050

internal sealed record BucketDraft(string Key, string DisplayName, decimal CurrentValue);

internal sealed record PortfolioAnalysis(
    decimal Equity,
    IReadOnlyList<PortfolioBucket> Buckets,
    decimal Top1,
    decimal Top3,
    decimal Top5,
    decimal Hhi,
    string Result,
    IReadOnlyList<string> Warnings,
    bool CanCalculate);

internal sealed record CliOptions(EToroEnvironment Account, bool IncludeCash)
{
    public bool IsValid => ErrorMessage is null;
    public string? ErrorMessage { get; init; }

    public static CliOptions Invalid(string message)
    {
        return new CliOptions(EToroEnvironment.Real, IncludeCash: true) { ErrorMessage = message };
    }
}
