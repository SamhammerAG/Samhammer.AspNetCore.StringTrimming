namespace Samhammer.AspNetCore.StringTrimming
{
    /// <summary>
    /// Opts a property, field or action parameter out of the global string trimming.
    /// Use it for values where leading or trailing whitespace is meaningful (e.g. markdown, templates, secrets, signatures).
    /// JSON (System.Text.Json and Newtonsoft.Json): supported on string properties, fields, constructor parameters (attribute on the matching property, e.g. [property: NoTrim])
    /// and on collections/dictionaries of strings. No extra converter is needed, Newtonsoft.Json reads the attribute in TrimStringContractResolver.
    /// Complex types (nothing inside the member is trimmed): System.Text.Json only, with Newtonsoft.Json the attribute is ignored there.
    /// Model binding (route, query, form): supported on action parameters and bound properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class NoTrimAttribute : Attribute
    {
    }
}
