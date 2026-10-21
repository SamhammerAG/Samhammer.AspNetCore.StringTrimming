using Newtonsoft.Json.Serialization;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    /// <summary>
    /// Wraps the value provider of a string member and trims the value when it is set during deserialization.
    /// </summary>
    internal sealed class TrimStringValueProvider : IValueProvider
    {
        private readonly IValueProvider inner;

        public TrimStringValueProvider(IValueProvider inner)
        {
            this.inner = inner;
        }

        public void SetValue(object target, object value)
        {
            inner.SetValue(target, value is string text ? text.Trim() : value);
        }

        public object GetValue(object target)
        {
            return inner.GetValue(target);
        }
    }
}
