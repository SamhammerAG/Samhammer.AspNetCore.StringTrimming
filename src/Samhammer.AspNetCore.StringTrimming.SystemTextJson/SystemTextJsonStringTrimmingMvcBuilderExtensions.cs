using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Samhammer.AspNetCore.StringTrimming.SystemTextJson
{
    public static class SystemTextJsonStringTrimmingMvcBuilderExtensions
    {
        /// <summary>
        /// Trims leading and trailing whitespace of all string inputs: JSON bodies read with System.Text.Json as well as route, query and form values.
        /// Use <see cref="NoTrimAttribute"/> to opt out single members or parameters.
        /// Applied after all other configuration of <see cref="JsonOptions"/>, so it can be called before or after AddJsonOptions().
        /// </summary>
        public static IMvcBuilder AddSystemTextJsonStringTrimming(this IMvcBuilder builder)
        {
            builder.AddStringTrimmingModelBinder();
            builder.Services.PostConfigure<JsonOptions>(options => options.JsonSerializerOptions.AddStringTrimming());

            return builder;
        }
    }
}
