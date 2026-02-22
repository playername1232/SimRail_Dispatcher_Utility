using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimRailDispatcherUtility.Model.StationServiceModels;

public sealed class SimRailDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        var str = reader.GetString();

        if (string.IsNullOrWhiteSpace(str))
            return default;

        str = str.Trim();

        if (DateTime.TryParseExact(
                str,
                Format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var dt))
        {
            return dt;
        }

        throw new JsonException($"Unsupported datetime: '{str}' (len={str.Length})");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}