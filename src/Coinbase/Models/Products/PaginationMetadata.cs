using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>Pagination cursors returned by <c>list-products</c>.</summary>
public sealed class PaginationMetadata
{
    [JsonPropertyName("prev_cursor")] public string? PrevCursor { get; set; }
    [JsonPropertyName("next_cursor")] public string? NextCursor { get; set; }
    [JsonPropertyName("has_next")] public bool? HasNext { get; set; }
    [JsonPropertyName("has_prev")] public bool? HasPrev { get; set; }
}
