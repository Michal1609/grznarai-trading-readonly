/*
 * Demo 03 - What-if Simulator
 * ---------------------------
 * Fetches the current eToro PnL snapshot and estimates how a hypothetical
 * price move in one instrument would affect total portfolio equity.
 *
 * Usage:
 *   dotnet run --project examples/Demo03.WhatIfSimulator
 *   dotnet run --project examples/Demo03.WhatIfSimulator -- --instrument-id 1004 --change -10
 *   dotnet run --project examples/Demo03.WhatIfSimulator -- --symbol TSLA --change 15
 *
 * Running without arguments uses the built-in sample scenario:
 *   account = real, instrument-id = 1832 (AMD), change = -10%
 *
 * Credentials:
 *   This demo uses the same UserSecretsId as the integration tests:
 *   GrznarAi.Trading.ReadOnly.Tests.Integration
 *
 *   If you already configured secrets for the test project in Visual Studio,
 *   this demo will read the same values.
 *
 *   You can also use appsettings.json or environment variables:
 *   EToroOptions__ApiKey=...  EToroOptions__UserKey=...
 *
 * The sample defaults to the real PnL endpoint because the public demo PnL
 * endpoint may reject otherwise valid credentials. Passing --account demo
 * is supported, but it depends on eToro account/API access.
 */

using System.Globalization;
using GrznarAi.Trading.ReadOnly.Etoro.Client;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Common;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Market;
using GrznarAi.Trading.ReadOnly.Etoro.Models.Pnl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

const EToroEnvironment BuiltInAccount = EToroEnvironment.Real;
const int BuiltInInstrumentId = 1832;
const string BuiltInInstrumentDisplayName = "AMD";
const decimal BuiltInChangePercent = -10m;

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
    Console.Error.WriteLine("Set credentials with user secrets, appsettings.json, or environment variables before running the demo.");
    return 2;
}

var services = new ServiceCollection();
services.AddEToro(configuration);

await using var serviceProvider = services.BuildServiceProvider(
    new ServiceProviderOptions { ValidateOnBuild = true });

var trading = serviceProvider.GetRequiredService<IEToroTradingClient>();
var market = serviceProvider.GetRequiredService<IEToroMarketDataClient>();

var target = options.InstrumentId.HasValue
    ? new InstrumentTarget(options.InstrumentId.Value, options.InstrumentDisplayName ?? $"Instrument {options.InstrumentId.Value}")
    : await ResolveSymbolAsync(market, options.Symbol!);

if (target is null)
{
    Console.Error.WriteLine($"Could not resolve symbol '{options.Symbol}' to an instrument ID.");
    return 2;
}

PnlResponse pnl;
try
{
    pnl = await trading.GetPnlAsync(options.Account);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error fetching {FormatAccount(options.Account)} PnL: {ex.Message}");
    return 1;
}

var result = Simulate(pnl, target, options.ChangePercent);
PrintReport(result, options.Account, target, options.ChangePercent);

return result.ExitCode;

static CliOptions ParseArgs(string[] args)
{
    if (args.Length == 0)
    {
        return new CliOptions(
            BuiltInAccount,
            Symbol: null,
            BuiltInInstrumentId,
            BuiltInChangePercent,
            BuiltInInstrumentDisplayName);
    }

    var account = EToroEnvironment.Real;
    string? symbol = null;
    int? instrumentId = null;
    decimal? changePercent = null;

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
        else if (arg.Equals("--symbol", StringComparison.OrdinalIgnoreCase))
        {
            if (++i >= args.Length || string.IsNullOrWhiteSpace(args[i]))
                return CliOptions.Invalid("Missing value for --symbol.");

            symbol = args[i].Trim();
        }
        else if (arg.Equals("--instrument-id", StringComparison.OrdinalIgnoreCase))
        {
            if (++i >= args.Length)
                return CliOptions.Invalid("Missing value for --instrument-id.");

            if (!int.TryParse(args[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedId) || parsedId <= 0)
                return CliOptions.Invalid($"Invalid --instrument-id value '{args[i]}'. Expected a positive integer.");

            instrumentId = parsedId;
        }
        else if (arg.Equals("--change", StringComparison.OrdinalIgnoreCase))
        {
            if (++i >= args.Length)
                return CliOptions.Invalid("Missing value for --change.");

            if (!decimal.TryParse(args[i], NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedChange))
                return CliOptions.Invalid($"Invalid --change value '{args[i]}'. Expected a percentage number such as -10 or 15.");

            changePercent = parsedChange;
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

    if (symbol is null && instrumentId is null)
        return CliOptions.Invalid("Specify either --symbol or --instrument-id.");
    if (symbol is not null && instrumentId is not null)
        return CliOptions.Invalid("Specify only one target: --symbol or --instrument-id.");
    if (!changePercent.HasValue)
        return CliOptions.Invalid("Specify --change as a percentage number.");
    if (changePercent.Value < -100m)
        return CliOptions.Invalid("--change cannot be less than -100.");

    return new CliOptions(account, symbol, instrumentId, changePercent.Value, InstrumentDisplayName: null);
}

static async Task<InstrumentTarget?> ResolveSymbolAsync(IEToroMarketDataClient market, string symbol)
{
    var response = await market.SearchInstrumentsAsync(new InstrumentSearchRequest
    {
        Fields =
        [
            InstrumentFields.InstrumentId,
            InstrumentFields.Symbol,
            InstrumentFields.InternalSymbolFull,
            InstrumentFields.DisplayName
        ],
        InternalSymbolFull = symbol,
        PageSize = 20,
    });

    var match = response.Instruments.FirstOrDefault(i =>
        i.InstrumentId.HasValue &&
        string.Equals(i.InternalSymbolFull, symbol, StringComparison.OrdinalIgnoreCase));

    if (match?.InstrumentId is null)
        return null;

    var displayName =
        match.InternalSymbolFull ??
        match.Symbol ??
        match.DisplayName ??
        $"Instrument {match.InstrumentId.Value}";

    return new InstrumentTarget(match.InstrumentId.Value, displayName);
}

static SimulationResult Simulate(PnlResponse pnl, InstrumentTarget target, decimal changePercent)
{
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
        return SimulationResult.CannotCalculate(
            "Cannot run simulation because equity is zero or negative.");
    }

    var matchingPositions = pnl.Positions
        .Where(p => p.InstrumentId == target.InstrumentId)
        .Select(p => new PositionScenario(
            p.PositionId,
            p.Amount + p.PnL,
            (p.Amount + p.PnL) * changePercent / 100m))
        .Concat(pnl.MirrorPortfolios.SelectMany(m => m.Positions
            .Where(p => p.InstrumentId == target.InstrumentId)
            .Select(p => new PositionScenario(
                p.PositionId,
                p.Amount + p.PnL,
                (p.Amount + p.PnL) * changePercent / 100m))))
        .ToList();

    if (matchingPositions.Count == 0)
    {
        return SimulationResult.NotFound(
            $"Instrument {target.DisplayName} ({target.InstrumentId}) is not open in the portfolio.",
            equity);
    }

    var targetExposure = matchingPositions.Sum(p => p.CurrentValue);
    var changeRatio = changePercent / 100m;
    var impactAmount = targetExposure * changeRatio;
    var newEquity = equity + impactAmount;
    var portfolioImpactPct = impactAmount / equity * 100m;
    var currentTargetWeightPct = targetExposure / equity * 100m;
    var newTargetExposure = targetExposure + impactAmount;
    decimal? newTargetWeightPct = newEquity == 0m ? null : newTargetExposure / newEquity * 100m;

    return SimulationResult.Success(
        equity,
        targetExposure,
        currentTargetWeightPct,
        impactAmount,
        portfolioImpactPct,
        newEquity,
        newTargetExposure,
        newTargetWeightPct,
        matchingPositions);
}

static void PrintReport(
    SimulationResult result,
    EToroEnvironment account,
    InstrumentTarget target,
    decimal changePercent)
{
    Console.WriteLine("What-if Simulator");
    Console.WriteLine($"Account: {FormatAccount(account)}");
    Console.WriteLine($"Scenario: {target.DisplayName} {SignedPercent(changePercent)}");
    Console.WriteLine();

    if (!result.CanCalculate)
    {
        Console.WriteLine(result.Message);
        return;
    }

    if (result.TargetNotFound)
    {
        Console.WriteLine($"Current equity: {Usd(result.CurrentEquity)}");
        Console.WriteLine(result.Message);
        Console.WriteLine();
        PrintDisclaimer();
        return;
    }

    Console.WriteLine($"{"Current equity:",-24} {Usd(result.CurrentEquity),14}");
    Console.WriteLine($"{$"Current {target.DisplayName} value:",-24} {Usd(result.CurrentTargetExposure),14}");
    Console.WriteLine($"{$"Current {target.DisplayName} weight:",-24} {Percent(result.CurrentTargetWeightPct),14}");
    Console.WriteLine();
    Console.WriteLine($"{"Estimated impact:",-24} {Usd(result.ImpactAmount),14}");
    Console.WriteLine($"{"Portfolio impact:",-24} {SignedPercent(result.PortfolioImpactPct),14}");
    Console.WriteLine($"{"New equity:",-24} {Usd(result.NewEquity),14}");
    Console.WriteLine($"{$"New {target.DisplayName} value:",-24} {Usd(result.NewTargetExposure),14}");
    Console.WriteLine($"{$"New {target.DisplayName} weight:",-24} {PercentOrNotAvailable(result.NewTargetWeightPct),14}");
    Console.WriteLine();
    Console.WriteLine("Matching positions:");

    for (var i = 0; i < result.Positions.Count; i++)
    {
        var position = result.Positions[i];
        Console.WriteLine(
            $"{i + 1}. Position {position.PositionId,-12} {Usd(position.CurrentValue),14}   impact {Usd(position.ImpactAmount)}");
    }

    Console.WriteLine();
    PrintDisclaimer();
}

static void PrintDisclaimer()
{
    Console.WriteLine(
        "Note: This is a simplified mark-to-market simulation. It does not include spread, fees, overnight fees, tax, slippage, margin calls, or stop-loss/take-profit execution.");
}

static void PrintUsage()
{
    Console.WriteLine("What-if Simulator");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project examples/Demo03.WhatIfSimulator");
    Console.WriteLine("  dotnet run --project examples/Demo03.WhatIfSimulator -- --symbol TSLA --change -10");
    Console.WriteLine("  dotnet run --project examples/Demo03.WhatIfSimulator -- --instrument-id 1004 --change 15");
    Console.WriteLine();
    Console.WriteLine("No-argument default:");
    Console.WriteLine($"  account={FormatAccount(BuiltInAccount)}, instrument-id={BuiltInInstrumentId} ({BuiltInInstrumentDisplayName}), change={SignedPercent(BuiltInChangePercent)}");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --account demo|real       Account endpoint to query. Default: real.");
    Console.WriteLine("  --symbol <symbol>         Resolve an eToro symbol to an instrument ID.");
    Console.WriteLine("  --instrument-id <id>      Use an instrument ID directly.");
    Console.WriteLine("  --change <percent>        Hypothetical price change. Example: -10 or 15.");
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
    return value.ToString("F2", CultureInfo.InvariantCulture) + "%";
}

static string PercentOrNotAvailable(decimal? value)
{
    return value.HasValue ? Percent(value.Value) : "n/a";
}

static string SignedPercent(decimal value)
{
    return (value >= 0m ? "+" : string.Empty) +
           value.ToString("F2", CultureInfo.InvariantCulture) +
           "%";
}

internal sealed record CliOptions(
    EToroEnvironment Account,
    string? Symbol,
    int? InstrumentId,
    decimal ChangePercent,
    string? InstrumentDisplayName = null)
{
    public bool IsValid => ErrorMessage is null;
    public string? ErrorMessage { get; init; }

    public static CliOptions Invalid(string message)
    {
        return new CliOptions(EToroEnvironment.Real, Symbol: null, InstrumentId: null, ChangePercent: 0m)
        {
            ErrorMessage = message
        };
    }
}

internal sealed record InstrumentTarget(int InstrumentId, string DisplayName);

internal sealed record PositionScenario(long PositionId, decimal CurrentValue, decimal ImpactAmount);

internal sealed record SimulationResult(
    bool CanCalculate,
    bool TargetNotFound,
    int ExitCode,
    string Message,
    decimal CurrentEquity,
    decimal CurrentTargetExposure,
    decimal CurrentTargetWeightPct,
    decimal ImpactAmount,
    decimal PortfolioImpactPct,
    decimal NewEquity,
    decimal NewTargetExposure,
    decimal? NewTargetWeightPct,
    IReadOnlyList<PositionScenario> Positions)
{
    public static SimulationResult CannotCalculate(string message)
    {
        return new SimulationResult(
            CanCalculate: false,
            TargetNotFound: false,
            ExitCode: 1,
            Message: message,
            CurrentEquity: 0m,
            CurrentTargetExposure: 0m,
            CurrentTargetWeightPct: 0m,
            ImpactAmount: 0m,
            PortfolioImpactPct: 0m,
            NewEquity: 0m,
            NewTargetExposure: 0m,
            NewTargetWeightPct: null,
            Positions: []);
    }

    public static SimulationResult NotFound(string message, decimal equity)
    {
        return new SimulationResult(
            CanCalculate: true,
            TargetNotFound: true,
            ExitCode: 2,
            Message: message,
            CurrentEquity: equity,
            CurrentTargetExposure: 0m,
            CurrentTargetWeightPct: 0m,
            ImpactAmount: 0m,
            PortfolioImpactPct: 0m,
            NewEquity: equity,
            NewTargetExposure: 0m,
            NewTargetWeightPct: 0m,
            Positions: []);
    }

    public static SimulationResult Success(
        decimal equity,
        decimal targetExposure,
        decimal currentTargetWeightPct,
        decimal impactAmount,
        decimal portfolioImpactPct,
        decimal newEquity,
        decimal newTargetExposure,
        decimal? newTargetWeightPct,
        IReadOnlyList<PositionScenario> positions)
    {
        return new SimulationResult(
            CanCalculate: true,
            TargetNotFound: false,
            ExitCode: 0,
            Message: string.Empty,
            CurrentEquity: equity,
            CurrentTargetExposure: targetExposure,
            CurrentTargetWeightPct: currentTargetWeightPct,
            ImpactAmount: impactAmount,
            PortfolioImpactPct: portfolioImpactPct,
            NewEquity: newEquity,
            NewTargetExposure: newTargetExposure,
            NewTargetWeightPct: newTargetWeightPct,
            Positions: positions);
    }
}
