using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;

public sealed class ListAccountsResponse
{
    [JsonPropertyName("accounts")] public List<Account>? Accounts { get; set; }
    [JsonPropertyName("has_next")] public bool? HasNext { get; set; }
    [JsonPropertyName("cursor")] public string? Cursor { get; set; }
    [JsonPropertyName("size")] public int? Size { get; set; }
}
