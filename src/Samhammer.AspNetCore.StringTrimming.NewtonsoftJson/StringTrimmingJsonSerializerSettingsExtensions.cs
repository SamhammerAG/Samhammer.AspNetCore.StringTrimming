using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    public static class StringTrimmingJsonSerializerSettingsExtensions
    {
        /// <summary>
        /// Replaces the contract resolver with a <see cref="TrimStringContractResolver"/> that keeps the settings of the current resolver (e.g. the camel case naming of ASP.NET Core).
        /// Supported current resolvers: none, <see cref="DefaultContractResolver"/>, <see cref="CamelCasePropertyNamesContractResolver"/> and <see cref="TrimStringContractResolver"/> (incl. derived ones).
        /// A custom resolver must derive from <see cref="TrimStringContractResolver"/> instead.
        /// </summary>
        public static JsonSerializerSettings AddStringTrimming(this JsonSerializerSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            settings.ContractResolver = settings.ContractResolver switch
            {
                TrimStringContractResolver current => current,
                null => new TrimStringContractResolver(),
                CamelCasePropertyNamesContractResolver => new TrimStringContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = true, OverrideSpecifiedNames = true },
                },
                DefaultContractResolver current when current.GetType() == typeof(DefaultContractResolver) => CopyFrom(current),
                var current => throw new InvalidOperationException(
                    $"String trimming can not be added to the contract resolver {current.GetType().FullName}. Derive it from {nameof(TrimStringContractResolver)} instead."),
            };

            return settings;
        }

        private static TrimStringContractResolver CopyFrom(DefaultContractResolver resolver)
        {
            return new TrimStringContractResolver
            {
                NamingStrategy = resolver.NamingStrategy,
                IgnoreSerializableAttribute = resolver.IgnoreSerializableAttribute,
                IgnoreSerializableInterface = resolver.IgnoreSerializableInterface,
                IgnoreIsSpecifiedMembers = resolver.IgnoreIsSpecifiedMembers,
                IgnoreShouldSerializeMembers = resolver.IgnoreShouldSerializeMembers,
                SerializeCompilerGeneratedMembers = resolver.SerializeCompilerGeneratedMembers,
            };
        }
    }
}
