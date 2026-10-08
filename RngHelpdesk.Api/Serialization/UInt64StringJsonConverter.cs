using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RngHelpdesk.Api.Serialization;

/// <summary>
/// Writes <see cref="ulong"/> values as JSON strings (Discord snowflakes exceed JS's
/// Number.MAX_SAFE_INTEGER) and reads either strings or numbers. Nullable ulong is covered
/// automatically by System.Text.Json's nullable wrapper.
/// </summary>
public sealed class UInt64StringJsonConverter : JsonConverter<ulong>
{
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetUInt64();
            case JsonTokenType.String:
                var text = reader.GetString();
                if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                    return value;
                throw new JsonException($"'{text}' is not a valid unsigned 64-bit integer.");
            default:
                throw new JsonException($"Unexpected token {reader.TokenType} when reading a ulong.");
        }
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));

    public override ulong ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ulong.Parse(reader.GetString()!, NumberStyles.None, CultureInfo.InvariantCulture);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        => writer.WritePropertyName(value.ToString(CultureInfo.InvariantCulture));
}
