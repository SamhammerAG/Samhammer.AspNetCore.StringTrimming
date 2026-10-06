using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Samhammer.AspNetCore.StringTrimming.SystemTextJson
{
    /// <summary>
    /// Provides a copy of the options without <see cref="TrimStringJsonConverter"/>, cached per options instance.
    /// </summary>
    internal static class NoTrimJsonSerializerOptions
    {
        private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> Cache = new();

        public static JsonSerializerOptions Get(JsonSerializerOptions options)
        {
            if (!options.Converters.OfType<TrimStringJsonConverter>().Any())
            {
                return options;
            }

            return Cache.GetValue(options, CreateWithoutTrimming);
        }

        private static JsonSerializerOptions CreateWithoutTrimming(JsonSerializerOptions options)
        {
            var copy = new JsonSerializerOptions(options);

            foreach (var converter in copy.Converters.OfType<TrimStringJsonConverter>().ToList())
            {
                copy.Converters.Remove(converter);
            }

            return copy;
        }
    }
}
