using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Orders;

/// <summary>Breakdown of commission components for an order or fill.</summary>
public sealed class CommissionDetailTotal
{
    [JsonPropertyName("total_commission")] public string? TotalCommission { get; set; }
    [JsonPropertyName("gst_commission")] public string? GstCommission { get; set; }
    [JsonPropertyName("withholding_commission")] public string? WithholdingCommission { get; set; }
    [JsonPropertyName("client_commission")] public string? ClientCommission { get; set; }
    [JsonPropertyName("venue_commission")] public string? VenueCommission { get; set; }
    [JsonPropertyName("regulatory_commission")] public string? RegulatoryCommission { get; set; }
    [JsonPropertyName("clearing_commission")] public string? ClearingCommission { get; set; }
}
