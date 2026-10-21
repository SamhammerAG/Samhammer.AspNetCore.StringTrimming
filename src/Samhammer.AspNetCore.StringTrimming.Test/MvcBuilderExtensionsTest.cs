using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Samhammer.AspNetCore.StringTrimming.ModelBinding;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.Test
{
    public class MvcBuilderExtensionsTest
    {
        [Fact]
        public void AddStringTrimmingModelBinder_RegistersModelBinderOnceAtFirstPosition()
        {
            var services = new ServiceCollection().AddLogging();
            services.AddControllers().AddStringTrimmingModelBinder().AddStringTrimmingModelBinder();

            using var provider = services.BuildServiceProvider();

            var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
            mvcOptions.ModelBinderProviders[0].Should().BeOfType<TrimStringModelBinderProvider>();
            mvcOptions.ModelBinderProviders.OfType<TrimStringModelBinderProvider>().Should().HaveCount(1);
        }
    }
}
