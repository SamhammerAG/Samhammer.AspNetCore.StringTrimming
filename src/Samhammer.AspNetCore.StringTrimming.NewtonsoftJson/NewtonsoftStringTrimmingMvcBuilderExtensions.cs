using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson
{
    public static class NewtonsoftStringTrimmingMvcBuilderExtensions
    {
        /// <summary>
        /// Trims leading and trailing whitespace of all string inputs: JSON bodies read with Newtonsoft.Json as well as route, query and form values.
        /// Use <see cref="NoTrimAttribute"/> to opt out single members or parameters.
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
