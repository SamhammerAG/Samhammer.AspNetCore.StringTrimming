using System.Globalization;
using Newtonsoft.Json;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    /// <summary>
    /// Reads string collections (string[], List, IEnumerable, ICollection, IList, IReadOnlyList, HashSet, ...) item by item with
    /// <see cref="JsonReader.ReadAsString"/>, which is what Newtonsoft.Json does without a converter, and trims the items.
    /// Writing uses the default behavior.
    /// </summary>
    internal sealed class StringCollectionJsonConverter : JsonConverter
    {
        public static readonly StringCollectionJsonConverter Trimming = new(true);

        public static readonly StringCollectionJsonConverter Untrimmed = new(false);

        private readonly bool trim;

        private StringCollectionJsonConverter(bool trim)
        {
            this.trim = trim;
        }

        public override bool CanWrite => false;

        public static bool IsSupported(Type type)
        {
            if (type == typeof(string[]))
            {
                return true;
            }

            if (type.IsArray || !typeof(IEnumerable<string>).IsAssignableFrom(type))
            {
                return false;
            }

            return type.IsAssignableFrom(typeof(List<string>))
                || (typeof(ICollection<string>).IsAssignableFrom(type) && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null);
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

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonSerializationException($"Unexpected token {reader.TokenType} when reading {objectType}, expected StartArray.");
            }

            var collection = GetCollection(objectType, existingValue);

            while (true)
            {
                var value = ReadStringValue(reader, objectType);

                if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }

                collection.Add(trim ? value?.Trim() : value);
            }

            return objectType.IsArray ? ((List<string>)collection).ToArray() : collection;
        }

        // ReadAsString normally returns String or Null tokens. Primitive tokens are converted like Newtonsoft.Json does for string targets.
        internal static string ReadStringValue(JsonReader reader, Type objectType)
        {
            var value = reader.ReadAsString();

            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Null:
                case JsonToken.EndArray:
                    return value;
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Boolean:
                case JsonToken.Date:
                    return Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
                default:
                    throw new JsonSerializationException($"Unexpected token {reader.TokenType} when reading a string of {objectType}.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        // Like Newtonsoft.Json: existing collections are reused (e.g. initialized properties), arrays are always created new
        private static ICollection<string> GetCollection(Type objectType, object existingValue)
        {
            if (objectType.IsArray)
            {
                return new List<string>();
            }

            if (existingValue is ICollection<string> { IsReadOnly: false } existing)
            {
                return existing;
            }

            return objectType.IsAssignableFrom(typeof(List<string>))
                ? new List<string>()
                : (ICollection<string>)Activator.CreateInstance(objectType);
        }
    }
}
