using System.Text.Json;
using System.Text.Json.Serialization;

namespace RngHelpdesk.DiscordBot.Api.Serialization;

/// <summary>Writes ulong as a JSON string (snowflakes exceed JS safe integers, ADR-0007); reads string or number.</summary>
public sealed class UInt64StringJsonConverter : JsonConverter<ulong>
{
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();
            if (ulong.TryParse(text, out var parsed))
                return parsed;
            throw new JsonException($"'{text}' is not a valid unsigned 64-bit integer.");
        }

        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetUInt64();

        throw new JsonException($"Unexpected token {reader.TokenType} when reading a ulong.");
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
