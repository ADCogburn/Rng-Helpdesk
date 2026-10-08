using System.Text.Json;
using RngHelpdesk.Api.Serialization;

namespace RngHelpdesk.Api.Tests;

public class UInt64StringJsonConverterTests
{
    private sealed record Payload(ulong Id, ulong? OptionalId);

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new UInt64StringJsonConverter() }
    };

    [Fact]
    public void Write_EmitsStringWithoutPrecisionLoss()
    {
        var json = JsonSerializer.Serialize(new Payload(ulong.MaxValue, 123456789012345678UL), Options);

        Assert.Contains("\"id\":\"18446744073709551615\"", json);
        Assert.Contains("\"optionalId\":\"123456789012345678\"", json);
    }

    [Fact]
    public void Write_NullableNull_EmitsNull()
    {
        var json = JsonSerializer.Serialize(new Payload(1, null), Options);

        Assert.Contains("\"optionalId\":null", json);
    }

    [Fact]
    public void Read_AcceptsStringAndNumber()
    {
        var p = JsonSerializer.Deserialize<Payload>(
            """{"id":"123456789012345678","optionalId":222222222}""", Options)!;

        Assert.Equal(123456789012345678UL, p.Id);
        Assert.Equal(222222222UL, p.OptionalId);
    }

    [Fact]
    public void Read_NullableNull_IsNull()
    {
        var p = JsonSerializer.Deserialize<Payload>("""{"id":"1","optionalId":null}""", Options)!;

        Assert.Null(p.OptionalId);
    }

    [Theory]
    [InlineData("""{"id":"abc"}""")]
    [InlineData("""{"id":"-1"}""")]
    [InlineData("""{"id":"1.5"}""")]
    [InlineData("""{"id":true}""")]
    [InlineData("""{"id":-1}""")]
    public void Read_InvalidValue_Throws(string json)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Payload>(json, Options));
    }
}
