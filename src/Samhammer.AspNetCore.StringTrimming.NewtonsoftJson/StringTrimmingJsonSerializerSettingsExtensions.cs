using Newtonsoft.Json;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    public static class StringTrimmingJsonSerializerSettingsExtensions
    {
        /// <summary>
        /// Adds <see cref="TrimStringJsonConverter"/> to the settings. Calling it multiple times has no further effect.
        /// </summary>
        public static JsonSerializerSettings AddStringTrimming(this JsonSerializerSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (!settings.Converters.OfType<TrimStringJsonConverter>().Any())
            {
                settings.Converters.Add(new TrimStringJsonConverter());
            }

            return settings;
        }
    }
}
