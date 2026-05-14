using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Json;

/// <summary>
/// JSON converter that reads a numeric value sent as a JSON string or number into a <see cref="decimal"/>.
/// Writes back as a string to match APIs that use string encoding for numeric fields (e.g. Coinbase).
/// </summary>
public sealed class DecimalStringConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDecimal();

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return 0m;

            return decimal.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        throw new JsonException($"Unexpected token {reader.TokenType} for decimal.");
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}

/// <summary>Nullable variant of <see cref="DecimalStringConverter"/>.</summary>
public sealed class NullableDecimalStringConverter : JsonConverter<decimal?>
{
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDecimal();

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return null;

            return decimal.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        throw new JsonException($"Unexpected token {reader.TokenType} for nullable decimal.");
    }

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToString(CultureInfo.InvariantCulture));
    }
}
