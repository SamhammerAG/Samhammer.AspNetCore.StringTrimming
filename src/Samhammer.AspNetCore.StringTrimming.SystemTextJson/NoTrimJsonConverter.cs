using System.Text.Json;
using System.Text.Json.Serialization;

namespace Samhammer.AspNetCore.StringTrimming.SystemTextJson
{
    /// <summary>
    /// Applied to members marked with <see cref="NoTrimAttribute"/>: reads the member with the same options, but without <see cref="TrimStringJsonConverter"/>.
    /// </summary>
    internal sealed class NoTrimJsonConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader, NoTrimJsonSerializerOptions.Get(options));
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
