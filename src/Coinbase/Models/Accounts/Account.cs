using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;

public sealed class Account
{
    [JsonPropertyName("uuid")] public string? Uuid { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
    [JsonPropertyName("available_balance")] public AccountBalance? AvailableBalance { get; set; }
    [JsonPropertyName("default")] public bool? Default { get; set; }
    [JsonPropertyName("active")] public bool? Active { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset? CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTimeOffset? UpdatedAt { get; set; }
    [JsonPropertyName("deleted_at")] public DateTimeOffset? DeletedAt { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("ready")] public bool? Ready { get; set; }
    [JsonPropertyName("hold")] public AccountBalance? Hold { get; set; }
    [JsonPropertyName("retail_portfolio_id")] public string? RetailPortfolioId { get; set; }
    [JsonPropertyName("platform")] public string? Platform { get; set; }
}
