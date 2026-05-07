/*
 * Demo 01 — Portfolio Health Check
 * ─────────────────────────────────
 * Fetches the current real-account portfolio via the eToro PnL endpoint,
 * computes key metrics (equity, cash, invested, unrealized P/L, position
 * weights) and prints a health status.
 *
 * Note: the eToro public API only supports the real-account PnL endpoint.
 *       The demo endpoint (/trading/info/demo/pnl) is not available and
 *       returns 401 Unauthorized.
 *
 * Usage:
 *   dotnet run
 *
 * Credentials — choose one:
 *   A) User Secrets (recommended — keys stay out of source control):
 *        dotnet user-secrets set "EToroOptions:ApiKey"  "your-api-key"
 *        dotnet user-secrets set "EToroOptions:UserKey" "your-user-key"
 *   B) Environment variables:
 *        EToroOptions__ApiKey=...  EToroOptions__UserKey=...
 *   C) appsettings.json — edit ApiKey / UserKey directly (not safe for real keys)
 */

using GrznarAi.Trading.ReadOnly.Client;
using GrznarAi.Trading.ReadOnly.Models.Common;
using GrznarAi.Trading.ReadOnly.Models.Pnl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

// ── 1. Build DI host and register the eToro client ───────────────────────────
//
// Host.CreateApplicationBuilder reads (in order):
//   appsettings.json → user-secrets → environment variables
//
// AddEToro wires up:
//   • IEToroClient / IEToroTradingClient / IEToroMarketDataClient / …
//   • Auth handler  (injects x-api-key and x-user-key headers)
//   • Rate-limiter  (sliding-window, default 60 req/min)
//   • Transient-error retry handler

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
var market  = serviceProvider.GetRequiredService<IEToroMarketDataClient>();

// ── 2. Fetch PnL (real account) ───────────────────────────────────────────────

Console.WriteLine("Fetching real portfolio…");

PnlResponse pnl;
try
{
    pnl = await trading.GetPnlAsync(EToroEnvironment.Real);
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
catch (TaskCanceledException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

// ── 3. Compute metrics ────────────────────────────────────────────────────────
//
// Formulas derived from the eToro PnL API structure:
//   availableCash  = credit − (manualOrdersForOpen + mitOrders)
//   totalInvested  = positions + mirrorPositions + mirrorAllocated
//                  + manualOrdersForOpen + mitOrders + externalCosts
//   unrealizedPnL  = positions.PnL + mirrorPositions.PnL + mirrors.closedProfit
//   equity         = availableCash + totalInvested + unrealizedPnL

var manualOrdersAmount  = pnl.OrdersForOpen.Where(o => o.MirrorId == 0).Sum(o => o.Amount);
var manualExternalCosts = pnl.OrdersForOpen.Where(o => o.MirrorId == 0).Sum(o => o.TotalExternalCosts);
var mitOrdersAmount     = pnl.Orders.Sum(o => o.Amount);

var availableCash = pnl.Credit - (manualOrdersAmount + mitOrdersAmount);

var totalInvested =
    pnl.Positions.Sum(p => p.Amount)
    + pnl.MirrorPortfolios.Sum(m => m.Positions.Sum(p => p.Amount))
    + pnl.MirrorPortfolios.Sum(m => m.AvailableAmount - m.ClosedPositionsNetProfit)
    + manualOrdersAmount
    + mitOrdersAmount
    + manualExternalCosts;

var unrealizedPnL =
    pnl.Positions.Sum(p => p.PnL)
    + pnl.MirrorPortfolios.Sum(m => m.Positions.Sum(p => p.PnL))
    + pnl.MirrorPortfolios.Sum(m => m.ClosedPositionsNetProfit);

var equity = availableCash + totalInvested + unrealizedPnL;

// ── 4. Build flat position list (manual + all mirror sub-positions) ───────────

var allPositions = pnl.Positions
    .Select(p => new PositionEntry(p.InstrumentId, p.Amount + p.PnL))
    .Concat(pnl.MirrorPortfolios.SelectMany(m =>
        m.Positions.Select(p => new PositionEntry(p.InstrumentId, p.Amount + p.PnL))))
    .OrderByDescending(p => p.CurrentValue)
    .ToList();

int openPositions = allPositions.Count;
int pendingOrders = pnl.OrdersForOpen.Count + pnl.Orders.Count;

// ── 5. Resolve instrument symbols for top positions ───────────────────────────
//
// One additional GET to market-data/instruments enriches display names.
// Gracefully degrades to "Instrument #ID" if the call fails.

const int TopN = 3;

var topIds   = allPositions.Take(TopN).Select(p => p.InstrumentId).Distinct().ToList();
var nameById = new Dictionary<int, string>();

if (topIds.Count > 0)
{
    try
    {
        var meta = await market.GetInstrumentMetadataAsync(instrumentIds: topIds);
        foreach (var d in meta.InstrumentDisplayDatas)
            nameById[d.InstrumentId] = d.SymbolFull ?? d.InstrumentDisplayName ?? $"#{d.InstrumentId}";
    }
    catch
    {
        // Non-critical; falls back to ID display
    }
}

string Label(int id) =>
    nameById.TryGetValue(id, out var name) ? name : $"#{id}";

// ── 6. Compute weights and health status ──────────────────────────────────────

var weighted = allPositions
    .Select(p => (
        Label:     Label(p.InstrumentId),
        Value:     p.CurrentValue,
        WeightPct: equity > 0 ? Math.Max(0, p.CurrentValue) / equity * 100 : 0))
    .ToList();

var top1Weight = weighted.ElementAtOrDefault(0).WeightPct;
var top3Weight = weighted.Take(TopN).Sum(p => p.WeightPct);
var cashPct    = equity > 0 ? availableCash / equity * 100 : 0;
var unrealPct  = totalInvested != 0 ? unrealizedPnL / totalInvested * 100 : 0;

var (status, reason) = HealthStatus(equity, top1Weight, top3Weight, cashPct);

// ── 7. Print output ───────────────────────────────────────────────────────────

Console.WriteLine();
Console.WriteLine("Portfolio Health Check");
Console.WriteLine($"Account:   real");
Console.WriteLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine();
Console.WriteLine($"{"Equity:",-20} {Usd(equity),12}");
Console.WriteLine($"{"Available cash:",-20} {Usd(availableCash),12}  ({cashPct:F2}%)");
Console.WriteLine($"{"Total invested:",-20} {Usd(totalInvested),12}");
Console.WriteLine($"{"Unrealized P/L:",-20} {Usd(unrealizedPnL),12}  ({Signed(unrealPct)}{Math.Abs(unrealPct):F2}% vs invested)");
Console.WriteLine($"{"Open positions:",-20} {openPositions,12}");
Console.WriteLine($"{"Pending orders:",-20} {pendingOrders,12}");

if (weighted.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("Top positions:");
    for (int i = 0; i < Math.Min(TopN, weighted.Count); i++)
    {
        var (label, value, weightPct) = weighted[i];
        Console.WriteLine($"{i + 1,2}. {label,-8} {Usd(value),12}   {weightPct:F2}%");
    }

    Console.WriteLine();
    Console.WriteLine($"Top 1 concentration: {top1Weight:F2}%");
    Console.WriteLine($"Top 3 concentration: {top3Weight:F2}%");
}
else
{
    Console.WriteLine();
    Console.WriteLine("No open positions.");
}

Console.WriteLine();
Console.Write("Health status: ");
Console.ForegroundColor = status switch
{
    "CRITICAL"  => ConsoleColor.Red,
    "HIGH RISK" => ConsoleColor.Red,
    "WATCH"     => ConsoleColor.Yellow,
    _           => ConsoleColor.Green
};
Console.WriteLine(status);
Console.ResetColor();
Console.WriteLine($"Reason: {reason}");

return 0;

// ── Local helpers ─────────────────────────────────────────────────────────────

static string Usd(decimal v) =>
    v < 0 ? $"-${Math.Abs(v):N2}" : $"${v:N2}";

static string Signed(decimal v) => v >= 0 ? "+" : string.Empty;

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

static (string Status, string Reason) HealthStatus(
    decimal equity, decimal top1, decimal top3, decimal cash)
{
    if (equity <= 0)
        return ("CRITICAL", "Equity is zero or negative.");
    if (top1 >= 40)
        return ("HIGH RISK", $"Largest position is {top1:F2}% of equity (threshold: 40%).");
    if (top3 >= 70)
        return ("HIGH RISK", $"Top 3 positions are {top3:F2}% of equity (threshold: 70%).");
    if (cash < 2)
        return ("WATCH", $"Available cash is {cash:F2}% of equity — low liquidity (threshold: 2%).");
    if (top3 >= 50)
        return ("WATCH", $"Top 3 positions represent {top3:F2}% of portfolio equity (threshold: 50%).");
    return ("OK", "Portfolio metrics are within healthy ranges.");
}

// ── Internal types ────────────────────────────────────────────────────────────

internal sealed record PositionEntry(int InstrumentId, decimal CurrentValue);
