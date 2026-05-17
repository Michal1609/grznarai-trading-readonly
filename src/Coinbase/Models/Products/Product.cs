using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Coinbase.Models.Products;

/// <summary>
/// A Coinbase Advanced Trade product (trading pair).
/// Used as the response for <c>GET /api/v3/brokerage/products/{product_id}</c>
/// and as items inside <c>list-products</c>.
/// </summary>
public sealed class Product
{
    /// <summary>Trading pair identifier (e.g. 'BTC-USD').</summary>
    [JsonPropertyName("product_id")] public string? ProductId { get; set; }

    /// <summary>Current price in quote currency.</summary>
    [JsonPropertyName("price")] public string? Price { get; set; }

    [JsonPropertyName("price_percentage_change_24h")] public string? PricePercentageChange24h { get; set; }
    [JsonPropertyName("volume_24h")] public string? Volume24h { get; set; }
    [JsonPropertyName("volume_percentage_change_24h")] public string? VolumePercentageChange24h { get; set; }
    [JsonPropertyName("base_increment")] public string? BaseIncrement { get; set; }
    [JsonPropertyName("quote_increment")] public string? QuoteIncrement { get; set; }
    [JsonPropertyName("quote_min_size")] public string? QuoteMinSize { get; set; }
    [JsonPropertyName("quote_max_size")] public string? QuoteMaxSize { get; set; }
    [JsonPropertyName("base_min_size")] public string? BaseMinSize { get; set; }
    [JsonPropertyName("base_max_size")] public string? BaseMaxSize { get; set; }
    [JsonPropertyName("base_name")] public string? BaseName { get; set; }
    [JsonPropertyName("quote_name")] public string? QuoteName { get; set; }
    [JsonPropertyName("watched")] public bool? Watched { get; set; }
    [JsonPropertyName("is_disabled")] public bool? IsDisabled { get; set; }
    [JsonPropertyName("new")] public bool? New { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("cancel_only")] public bool? CancelOnly { get; set; }
    [JsonPropertyName("limit_only")] public bool? LimitOnly { get; set; }
    [JsonPropertyName("post_only")] public bool? PostOnly { get; set; }

    /// <summary>Product disabled for all market participants.</summary>
    [JsonPropertyName("trading_disabled")] public bool? TradingDisabled { get; set; }

    [JsonPropertyName("auction_mode")] public bool? AuctionMode { get; set; }

    /// <summary>Product type — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductType"/>.</summary>
    [JsonPropertyName("product_type")] public string? ProductType { get; set; }

    [JsonPropertyName("quote_currency_id")] public string? QuoteCurrencyId { get; set; }
    [JsonPropertyName("base_currency_id")] public string? BaseCurrencyId { get; set; }
    [JsonPropertyName("mid_market_price")] public string? MidMarketPrice { get; set; }

    /// <summary>Product ID of the corresponding unified book.</summary>
    [JsonPropertyName("alias")] public string? Alias { get; set; }

    /// <summary>Product IDs this product is an alias for.</summary>
    [JsonPropertyName("alias_to")] public List<string>? AliasTo { get; set; }

    [JsonPropertyName("base_display_symbol")] public string? BaseDisplaySymbol { get; set; }
    [JsonPropertyName("quote_display_symbol")] public string? QuoteDisplaySymbol { get; set; }

    /// <summary>For SPOT: tradability status. For FCM futures: whether the contract has expired.</summary>
    [JsonPropertyName("view_only")] public bool? ViewOnly { get; set; }

    [JsonPropertyName("price_increment")] public string? PriceIncrement { get; set; }
    [JsonPropertyName("display_name")] public string? DisplayName { get; set; }

    /// <summary>Venue — see <see cref="GrznarAi.Trading.ReadOnly.Coinbase.Models.Common.ProductVenue"/>.</summary>
    [JsonPropertyName("product_venue")] public string? ProductVenue { get; set; }

    [JsonPropertyName("approximate_quote_24h_volume")] public string? ApproximateQuote24hVolume { get; set; }
    [JsonPropertyName("new_at")] public DateTimeOffset? NewAt { get; set; }
    [JsonPropertyName("market_cap")] public string? MarketCap { get; set; }
    [JsonPropertyName("icon_color")] public string? IconColor { get; set; }
    [JsonPropertyName("icon_url")] public string? IconUrl { get; set; }
    [JsonPropertyName("display_name_overwrite")] public string? DisplayNameOverwrite { get; set; }
    [JsonPropertyName("is_alpha_testing")] public bool? IsAlphaTesting { get; set; }
    [JsonPropertyName("about_description")] public string? AboutDescription { get; set; }
    [JsonPropertyName("best_bid_price")] public string? BestBidPrice { get; set; }
    [JsonPropertyName("best_ask_price")] public string? BestAskPrice { get; set; }
    [JsonPropertyName("future_product_details")] public FutureProductDetails? FutureProductDetails { get; set; }
    [JsonPropertyName("fcm_trading_session_details")] public FcmTradingSessionDetails? FcmTradingSessionDetails { get; set; }
}
