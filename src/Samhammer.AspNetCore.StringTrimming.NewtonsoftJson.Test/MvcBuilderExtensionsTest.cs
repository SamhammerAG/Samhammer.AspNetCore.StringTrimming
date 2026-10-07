using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Samhammer.AspNetCore.StringTrimming.ModelBinding;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson.Test
{
    public class MvcBuilderExtensionsTest
    {
        [Fact]
        public void AddNewtonsoftJsonStringTrimming_RegistersModelBinderAndConverter()
        {
            var services = new ServiceCollection().AddLogging();
            services.AddControllers()
                .AddNewtonsoftJsonStringTrimming()
                .AddNewtonsoftJsonStringTrimming()
                .AddNewtonsoftJson(); // order does not matter, trimming is applied in PostConfigure

            using var provider = services.BuildServiceProvider();

            var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
            mvcOptions.ModelBinderProviders[0].Should().BeOfType<TrimStringModelBinderProvider>();

            var newtonsoftOptions = provider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value;
            newtonsoftOptions.SerializerSettings.Converters.OfType<TrimStringJsonConverter>().Should().HaveCount(1);
        }
    }
}
