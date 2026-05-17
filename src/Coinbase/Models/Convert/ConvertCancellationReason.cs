using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Convert;

public sealed class ConvertCancellationReason
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("preview_failure_reason")]
    public string? PreviewFailureReason { get; set; }

    [JsonPropertyName("new_failure_reason")]
    public string? NewFailureReason { get; set; }
}
