using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Futures-specific contract details returned inside a <see cref="Product"/>.</summary>
public sealed class FutureProductDetails
{
    [JsonPropertyName("venue")] public string? Venue { get; set; }
    [JsonPropertyName("contract_code")] public string? ContractCode { get; set; }
    [JsonPropertyName("contract_expiry")] public DateTimeOffset? ContractExpiry { get; set; }
    [JsonPropertyName("contract_size")] public string? ContractSize { get; set; }
    [JsonPropertyName("contract_expiry_type")] public string? ContractExpiryType { get; set; }
    [JsonPropertyName("perpetual_details")] public PerpetualDetails? PerpetualDetails { get; set; }
    [JsonPropertyName("contract_display_name")] public string? ContractDisplayName { get; set; }
    [JsonPropertyName("time_to_expiry_ms")] public long? TimeToExpiryMs { get; set; }
    [JsonPropertyName("non_crypto")] public bool? NonCrypto { get; set; }
    [JsonPropertyName("contract_expiry_name")] public string? ContractExpiryName { get; set; }
    [JsonPropertyName("twenty_four_by_seven")] public bool? TwentyFourBySeven { get; set; }
}
