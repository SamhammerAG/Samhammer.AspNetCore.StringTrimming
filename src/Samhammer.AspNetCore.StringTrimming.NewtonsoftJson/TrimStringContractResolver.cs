using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    /// <summary>
    /// Contract resolver that trims leading and trailing whitespace of all strings read from JSON:
    /// string properties and fields, string constructor parameters, string collections and the values of string dictionaries.
    /// Writing is not affected, dictionary keys and property names are not trimmed. Members marked with <see cref="NoTrimAttribute"/> are skipped.
    /// </summary>
    /// <remarks>
    /// Strings are trimmed after Newtonsoft.Json read them as usual instead of using a JsonConverter for string.
    /// A converter would change how Newtonsoft.Json reads strings (date-like strings would be parsed as dates, numbers would no longer be accepted for string properties).
    /// Not trimmed: a string as root value (e.g. [FromBody] string) and strings inside object, JToken or dynamic.
    /// </remarks>
    public class TrimStringContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            var noTrim = member.IsDefined(typeof(NoTrimAttribute), true);

            if (property.PropertyType == typeof(string))
            {
                if (!noTrim && property.ValueProvider != null)
                {
                    property.ValueProvider = new TrimStringValueProvider(property.ValueProvider);
                }
            }
            else if (noTrim && property.Converter == null)
            {
                property.Converter = GetUntrimmedConverter(property.PropertyType);
            }

            return property;
        }

        protected override JsonArrayContract CreateArrayContract(Type objectType)
        {
            var contract = base.CreateArrayContract(objectType);

            if (contract.Converter == null && StringCollectionJsonConverter.IsSupported(objectType))
            {
                contract.Converter = StringCollectionJsonConverter.Trimming;
            }

            return contract;
        }

        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);

            if (contract.Converter == null && StringDictionaryJsonConverter.IsSupported(objectType))
            {
                contract.Converter = StringDictionaryJsonConverter.Trimming;
            }

            return contract;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            var trimmedParameterIndexes = contract.CreatorParameters
                .Select((parameter, index) => (parameter, index))
                .Where(p => p.parameter.PropertyType == typeof(string) && !HasNoTrimAttribute(contract, p.parameter))
                .Select(p => p.index)
                .ToArray();

            if (trimmedParameterIndexes.Length == 0)
            {
                return contract;
            }

            var creator = contract.OverrideCreator ?? CreateConstructorCreator(objectType, contract.CreatorParameters.Count);
            if (creator == null)
            {
                return contract;
            }

            contract.OverrideCreator = args =>
            {
                foreach (var index in trimmedParameterIndexes)
                {
                    if (args[index] is string value)
                    {
                        args[index] = value.Trim();
                    }
                }

                return creator(args);
            };

            return contract;
        }

        private static JsonConverter GetUntrimmedConverter(Type propertyType)
        {
            if (StringCollectionJsonConverter.IsSupported(propertyType))
            {
                return StringCollectionJsonConverter.Untrimmed;
            }

            if (StringDictionaryJsonConverter.IsSupported(propertyType))
            {
                return StringDictionaryJsonConverter.Untrimmed;
            }

            // [NoTrim] on other types is not supported with Newtonsoft.Json, the strings inside are trimmed
            return null;
        }

        private static bool HasNoTrimAttribute(JsonObjectContract contract, JsonProperty creatorParameter)
        {
            if (creatorParameter.AttributeProvider?.GetAttributes(typeof(NoTrimAttribute), true).Count > 0)
            {
                return true;
            }

            var matchingProperty = contract.Properties.GetClosestMatchProperty(creatorParameter.PropertyName);
            return matchingProperty?.AttributeProvider?.GetAttributes(typeof(NoTrimAttribute), true).Count > 0;
        }

        // Newtonsoft.Json uses the single public constructor if there is no default constructor and no [JsonConstructor]
        private static ObjectConstructor<object> CreateConstructorCreator(Type objectType, int parameterCount)
        {
            var constructors = objectType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length != 1 || constructors[0].GetParameters().Length != parameterCount)
            {
                return null;
            }

            var constructor = constructors[0];
            return args => constructor.Invoke(args);
        }
    }
}
