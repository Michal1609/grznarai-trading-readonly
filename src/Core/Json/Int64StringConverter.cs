using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Json;

/// <summary>Reads a 64-bit integer encoded either as a JSON number or JSON string; writes as a number.</summary>
public sealed class Int64StringConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt64();

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return 0L;

            return long.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        throw new JsonException($"Unexpected token {reader.TokenType} for long.");
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}

/// <summary>Nullable variant of <see cref="Int64StringConverter"/>.</summary>
public sealed class NullableInt64StringConverter : JsonConverter<long?>
{
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt64();

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return null;

            return long.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        throw new JsonException($"Unexpected token {reader.TokenType} for nullable long.");
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteNumberValue(value.Value);
    }
}
