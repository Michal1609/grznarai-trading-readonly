using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>A single entry in an order's edit history, recording a price/size replace operation.</summary>
public sealed class OrderEditHistory
{
    [JsonPropertyName("price")] public string? Price { get; set; }
    [JsonPropertyName("size")] public string? Size { get; set; }
    [JsonPropertyName("replace_accept_timestamp")] public DateTimeOffset? ReplaceAcceptTimestamp { get; set; }
}
