using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Social;

public record PopularInvestorsResponse(
    [property: JsonPropertyName("totalItems")] int TotalItems,
    [property: JsonPropertyName("items")] IReadOnlyList<PopularInvestor> Items
);

public record PopularInvestor(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("gain")] decimal Gain,
    [property: JsonPropertyName("dailyGain")] decimal DailyGain,
    [property: JsonPropertyName("winRatio")] decimal WinRatio,
    [property: JsonPropertyName("riskScore")] int RiskScore,
    [property: JsonPropertyName("trades")] int Trades,
    [property: JsonPropertyName("copiers")] int Copiers,
    [property: JsonPropertyName("weeklyDd")] decimal WeeklyDrawdown,
    [property: JsonPropertyName("peakToValley")] decimal PeakToValley
);
