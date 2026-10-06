namespace Samhammer.AspNetCore.StringTrimming
{
    /// <summary>
    /// Opts a property, field or action parameter out of the global string trimming.
    /// Use it for values where leading or trailing whitespace is meaningful (e.g. markdown, templates, secrets, signatures).
    /// Supported for strings and collections/dictionaries of strings with System.Text.Json and Newtonsoft.Json,
    /// with System.Text.Json also for complex types (nothing inside the member is trimmed).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class NoTrimAttribute : Attribute
    {
    }
}
