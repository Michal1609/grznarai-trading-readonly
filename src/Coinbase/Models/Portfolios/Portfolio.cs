using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Portfolios;

public sealed class Portfolio
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("uuid")] public string? Uuid { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("deleted")] public bool? Deleted { get; set; }
}
