using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Watchlist;

[JsonConverter(typeof(JsonStringEnumConverter<WatchlistType>))]
public enum WatchlistType { Static, Dynamic, RecentlyInvested, Default }
