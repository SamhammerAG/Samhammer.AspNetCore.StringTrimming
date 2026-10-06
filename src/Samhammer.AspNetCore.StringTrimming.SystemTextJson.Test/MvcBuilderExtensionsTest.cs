using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Samhammer.AspNetCore.StringTrimming.ModelBinding;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.SystemTextJson.Test
{
    public class MvcBuilderExtensionsTest
    {
        [Fact]
        public void AddSystemTextJsonStringTrimming_RegistersModelBinderAndConverter()
        {
            var services = new ServiceCollection().AddLogging();
            services.AddControllers()
                .AddSystemTextJsonStringTrimming()
                .AddSystemTextJsonStringTrimming();

            using var provider = services.BuildServiceProvider();

            var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
            mvcOptions.ModelBinderProviders[0].Should().BeOfType<TrimStringModelBinderProvider>();
            mvcOptions.ModelBinderProviders.OfType<TrimStringModelBinderProvider>().Should().HaveCount(1);

            var jsonOptions = provider.GetRequiredService<IOptions<JsonOptions>>().Value;
            jsonOptions.JsonSerializerOptions.Converters.OfType<TrimStringJsonConverter>().Should().HaveCount(1);
        }
    }
}
