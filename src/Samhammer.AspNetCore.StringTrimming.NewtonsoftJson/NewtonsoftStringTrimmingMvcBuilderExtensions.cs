using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    public static class NewtonsoftStringTrimmingMvcBuilderExtensions
    {
        /// <summary>
        /// Trims leading and trailing whitespace of all string inputs: JSON bodies read with Newtonsoft.Json as well as route, query and form values.
        /// Opt-out: <see cref="NoTrimAttribute"/> for route, query and form, <c>[JsonConverter(typeof(NoTrimStringJsonConverter))]</c> for JSON properties.
        /// Applied after all other configuration of <see cref="MvcNewtonsoftJsonOptions"/>, so it can be called before or after AddNewtonsoftJson().
        /// </summary>
        public static IMvcBuilder AddNewtonsoftJsonStringTrimming(this IMvcBuilder builder)
        {
            builder.AddStringTrimmingModelBinder();
            builder.Services.PostConfigure<MvcNewtonsoftJsonOptions>(options => options.SerializerSettings.AddStringTrimming());

            return builder;
        }
    }
}
