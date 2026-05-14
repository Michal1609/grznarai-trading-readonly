using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

public sealed class ListPortfoliosResponse
{
    [JsonPropertyName("portfolios")] public List<Portfolio>? Portfolios { get; set; }
}
