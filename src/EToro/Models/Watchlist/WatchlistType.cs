using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Models.Watchlist;

[JsonConverter(typeof(JsonStringEnumConverter<WatchlistType>))]
public enum WatchlistType { Static, Dynamic, RecentlyInvested, Default }
