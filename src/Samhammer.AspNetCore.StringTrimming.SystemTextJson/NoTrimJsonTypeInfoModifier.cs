using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Samhammer.AspNetCore.StringTrimming.SystemTextJson
{
    /// <summary>
    /// Type info modifier that applies <see cref="NoTrimJsonConverter{T}"/> to all members marked with <see cref="NoTrimAttribute"/>.
    /// Members that already have their own converter (e.g. [JsonConverter]) are left unchanged.
    /// </summary>
    public static class NoTrimJsonTypeInfoModifier
    {
        public static void Modify(JsonTypeInfo typeInfo)
        {
            ArgumentNullException.ThrowIfNull(typeInfo);

            if (typeInfo.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }

            foreach (var property in typeInfo.Properties)
            {
                if (property.CustomConverter == null && property.AttributeProvider?.IsDefined(typeof(NoTrimAttribute), true) == true)
                {
                    var converterType = typeof(NoTrimJsonConverter<>).MakeGenericType(property.PropertyType);
                    property.CustomConverter = (JsonConverter)Activator.CreateInstance(converterType);
                }
            }
        }
    }
}
