using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Accounts;

public sealed class GetAccountResponse
{
    [JsonPropertyName("account")] public Account? Account { get; set; }
}
