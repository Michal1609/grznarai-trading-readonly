using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.UserInfo;

public record UserGainResponse(
    [property: JsonPropertyName("monthly")] IReadOnlyList<UserGainEntry>? Monthly,
    [property: JsonPropertyName("yearly")] IReadOnlyList<UserGainEntry>? Yearly
);

public record UserGainEntry(
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("gain")] decimal Gain
);

[JsonConverter(typeof(UserDailyGainResponseJsonConverter))]
public record UserDailyGainResponse(
    IReadOnlyList<UserGainEntry>? Daily,
    decimal? Gain
)
{
    public bool IsDaily => Daily is not null;
    public bool IsPeriod => Gain.HasValue;
}

internal sealed class UserDailyGainResponseJsonConverter : JsonConverter<UserDailyGainResponse>
{
    public override UserDailyGainResponse Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var entries = new List<UserGainEntry>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return new UserDailyGainResponse(entries, null);

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Unexpected daily gain entry shape.");

                entries.Add(ReadEntry(ref reader));
            }

            throw new JsonException("Unexpected end of daily gain response.");
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var gain = document.RootElement.TryGetProperty("gain", out var gainElement)
                ? gainElement.GetDecimal()
                : (decimal?)null;

            return new UserDailyGainResponse(null, gain);
        }

        throw new JsonException("Unexpected daily gain response shape.");
    }

    private static UserGainEntry ReadEntry(ref Utf8JsonReader reader)
    {
        DateTimeOffset timestamp = default;
        decimal gain = default;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new UserGainEntry(timestamp, gain);

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Unexpected daily gain entry property shape.");

            var propertyName = reader.GetString();
            if (!reader.Read())
                throw new JsonException("Unexpected end of daily gain entry.");

            switch (propertyName)
            {
                case "timestamp":
                    timestamp = reader.GetDateTimeOffset();
                    break;
                case "gain":
                    gain = reader.GetDecimal();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Unexpected end of daily gain entry.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        UserDailyGainResponse value,
        JsonSerializerOptions options)
    {
        if (value.Daily is not null)
        {
            writer.WriteStartArray();
            foreach (var entry in value.Daily)
            {
                writer.WriteStartObject();
                writer.WriteString("timestamp", entry.Timestamp);
                writer.WriteNumber("gain", entry.Gain);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            return;
        }

        writer.WriteStartObject();
        if (value.Gain.HasValue)
            writer.WriteNumber("gain", value.Gain.Value);
        writer.WriteEndObject();
    }
}
