using System.Text.Json;
using System.Text.Json.Serialization;

namespace Samhammer.AspNetCore.StringTrimming.SystemTextJson
{
    /// <summary>
    /// Trims leading and trailing whitespace of every string value read from JSON (properties, constructor parameters,
    /// collection items, dictionary values and root values). Writing is not affected, dictionary keys are not trimmed.
    /// </summary>
    public sealed class TrimStringJsonConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString()?.Trim();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }

        public override string ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString();
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value);
        }
    }
}
