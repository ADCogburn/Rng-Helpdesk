using System.Text.Json;
using System.Text.Json.Serialization;

namespace RngHelpdesk.DiscordBot.Api.Serialization;

public static class ApiJson
{
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new UInt64StringJsonConverter());
        return options;
    }
}
