using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Samhammer.AspNetCore.StringTrimming.SystemTextJson
{
    public static class StringTrimmingJsonSerializerOptionsExtensions
    {
        /// <summary>
        /// Adds string trimming to the options: <see cref="TrimStringJsonConverter"/> plus support for <see cref="NoTrimAttribute"/>.
        /// Calling it multiple times has no further effect. Must be called before the options are used for the first time.
        /// </summary>
        public static JsonSerializerOptions AddStringTrimming(this JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (options.Converters.OfType<TrimStringJsonConverter>().Any())
            {
                return options;
            }

            options.Converters.Add(new TrimStringJsonConverter());

            var resolver = options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
            options.TypeInfoResolver = resolver.WithAddedModifier(NoTrimJsonTypeInfoModifier.Modify);

            return options;
        }
    }
}
