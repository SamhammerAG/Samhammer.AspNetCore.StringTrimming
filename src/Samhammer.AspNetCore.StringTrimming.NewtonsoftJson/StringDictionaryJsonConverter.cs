using Newtonsoft.Json;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    /// <summary>
    /// Reads dictionaries with string keys and string values, reading the values with <see cref="JsonReader.ReadAsString"/>
    /// (like Newtonsoft.Json does without a converter) and trimming them. Keys are not trimmed. Writing uses the default behavior.
    /// </summary>
    internal sealed class StringDictionaryJsonConverter : JsonConverter
    {
        public static readonly StringDictionaryJsonConverter Trimming = new(true);

        public static readonly StringDictionaryJsonConverter Untrimmed = new(false);

        private readonly bool trim;

        private StringDictionaryJsonConverter(bool trim)
        {
            this.trim = trim;
        }

        public override bool CanWrite => false;

        public static bool IsSupported(Type type)
        {
            if (!typeof(IEnumerable<KeyValuePair<string, string>>).IsAssignableFrom(type))
            {
                return false;
            }

            return type.IsAssignableFrom(typeof(Dictionary<string, string>))
                || (typeof(IDictionary<string, string>).IsAssignableFrom(type) && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null);
        }

        public override bool CanConvert(Type objectType)
        {
            return IsSupported(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException($"Unexpected token {reader.TokenType} when reading {objectType}, expected StartObject.");
            }

            var dictionary = GetDictionary(objectType, existingValue);

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                    {
                        var key = (string)reader.Value;
                        var value = StringCollectionJsonConverter.ReadStringValue(reader, objectType);
                        dictionary[key] = trim ? value?.Trim() : value;
                        break;
                    }

                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return dictionary;
                    default:
                        throw new JsonSerializationException($"Unexpected token {reader.TokenType} when reading {objectType}.");
                }
            }

            throw new JsonSerializationException($"Unexpected end of JSON when reading {objectType}.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        // Like Newtonsoft.Json: existing dictionaries are reused (e.g. initialized properties)
        private static IDictionary<string, string> GetDictionary(Type objectType, object existingValue)
        {
            if (existingValue is IDictionary<string, string> { IsReadOnly: false } existing)
            {
                return existing;
            }

            return objectType.IsAssignableFrom(typeof(Dictionary<string, string>))
                ? new Dictionary<string, string>()
                : (IDictionary<string, string>)Activator.CreateInstance(objectType);
        }
    }
}
