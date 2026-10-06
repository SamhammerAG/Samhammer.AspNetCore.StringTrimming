using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Samhammer.AspNetCore.StringTrimming.ModelBinding;

namespace Samhammer.AspNetCore.StringTrimming
{
    public static class StringTrimmingMvcBuilderExtensions
    {
        /// <summary>
        /// Trims leading and trailing whitespace of route, query and form values. JSON bodies are not touched.
        /// Usually not called directly: AddSystemTextJsonStringTrimming() and AddNewtonsoftJsonStringTrimming() call it.
        /// </summary>
        public static IMvcBuilder AddStringTrimmingModelBinder(this IMvcBuilder builder)
        {
            builder.Services.PostConfigure<MvcOptions>(options =>
            {
                if (!options.ModelBinderProviders.OfType<TrimStringModelBinderProvider>().Any())
                {
                    options.ModelBinderProviders.Insert(0, new TrimStringModelBinderProvider());
                }
            });

            return builder;
        }
    }
}
