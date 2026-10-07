namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    /// <summary>
    /// Opts a single string property out of the trimming of JSON bodies:
    /// <c>[JsonConverter(typeof(NoTrimStringJsonConverter))]</c>. A property converter takes precedence over the global <see cref="TrimStringJsonConverter"/>.
    /// </summary>
    public class NoTrimStringJsonConverter : TrimStringJsonConverter
    {
        protected override string Convert(string value)
        {
            return value;
        }
    }
}
