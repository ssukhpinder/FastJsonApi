using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastJsonApi.Converters;

public sealed class UnixTimestampConverter : JsonConverter<DateTime>
{
    private static readonly DateTime s_epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var seconds = reader.GetInt64();
            return s_epoch.AddSeconds(seconds);
        }
        throw new JsonException("Expected Unix timestamp");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var seconds = (long)(value.ToUniversalTime() - s_epoch).TotalSeconds;
        writer.WriteNumberValue(seconds);
    }
}
