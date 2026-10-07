using Newtonsoft.Json;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    /// <summary>
    /// Trims leading and trailing whitespace of every string value read from JSON (properties, collection items, dictionary values, root values).
    /// Writing is not affected. Property names and dictionary keys are not trimmed.
    /// </summary>
    /// <remarks>
    /// Like the FUTO implementation, only JSON strings and null are accepted for string members.
    /// With a converter registered, Newtonsoft.Json no longer reads string members with ReadAsString, so numbers, booleans
    /// and date-like strings (with the default DateParseHandling.DateTime) arrive as other tokens and are rejected.
    /// </remarks>
    public class TrimStringJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.TokenType switch
            {
                JsonToken.Null => null,
                JsonToken.String => Convert((string)reader.Value),
                _ => throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing string."),
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException($"{nameof(TrimStringJsonConverter)} is only used for reading JSON request data.");
        }

        protected virtual string Convert(string value)
        {
            return value?.Trim();
        }
    }
}
