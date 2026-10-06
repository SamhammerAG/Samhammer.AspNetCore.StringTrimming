using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using Samhammer.AspNetCore.StringTrimming.ModelBinding;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson.Test
{
    public class MvcBuilderExtensionsTest
    {
        [Fact]
        public void AddNewtonsoftJsonStringTrimming_RegistersModelBinderAndContractResolver()
        {
            var services = new ServiceCollection().AddLogging();
            services.AddControllers()
                .AddNewtonsoftJsonStringTrimming()
                .AddNewtonsoftJson(); // order does not matter, trimming is applied in PostConfigure

            using var provider = services.BuildServiceProvider();

            var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
            mvcOptions.ModelBinderProviders[0].Should().BeOfType<TrimStringModelBinderProvider>();

            var newtonsoftOptions = provider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value;
            newtonsoftOptions.SerializerSettings.ContractResolver.Should().BeOfType<TrimStringContractResolver>()
                .Which.NamingStrategy.Should().BeOfType<CamelCaseNamingStrategy>();
        }
    }
}
