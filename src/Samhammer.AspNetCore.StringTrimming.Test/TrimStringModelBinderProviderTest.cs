using System.Reflection;
using AwesomeAssertions;
using Samhammer.AspNetCore.StringTrimming.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.Test
{
    public class TrimStringModelBinderProviderTest
    {
        private readonly TrimStringModelBinderProvider provider = new();

        private readonly EmptyModelMetadataProvider metadataProvider = new();

        [Theory]
        [InlineData(nameof(TestController.Query))]
        [InlineData(nameof(TestController.Route))]
        [InlineData(nameof(TestController.Form))]
        [InlineData(nameof(TestController.WithoutSource))]
        public void GetBinder_ForStringFromSupportedSource_ReturnsTrimBinder(string methodName)
        {
            var binder = provider.GetBinder(CreateContext(methodName));

            binder.Should().BeOfType<TrimStringModelBinder>();
        }

        [Theory]
        [InlineData(nameof(TestController.Body))]
        [InlineData(nameof(TestController.Header))]
        [InlineData(nameof(TestController.NoTrim))]
        [InlineData(nameof(TestController.NotAString))]
        public void GetBinder_ForUnsupportedParameter_ReturnsNull(string methodName)
        {
            var binder = provider.GetBinder(CreateContext(methodName));

            binder.Should().BeNull();
        }

        private TestModelBinderProviderContext CreateContext(string methodName)
        {
            var parameter = typeof(TestController).GetMethod(methodName)!.GetParameters().Single();
            var metadata = metadataProvider.GetMetadataForParameter(parameter);
            var bindingInfo = BindingInfo.GetBindingInfo(parameter.GetCustomAttributes(), metadata) ?? new BindingInfo();

            return new TestModelBinderProviderContext(metadata, bindingInfo, metadataProvider);
        }

        public class TestController
        {
            public void Query([FromQuery] string value)
            {
            }

            public void Route([FromRoute] string value)
            {
            }

            public void Form([FromForm] string value)
            {
            }

            public void WithoutSource(string value)
            {
            }

            public void Body([FromBody] string value)
            {
            }

            public void Header([FromHeader] string value)
            {
            }

            public void NoTrim([FromQuery][NoTrim] string value)
            {
            }

            public void NotAString([FromQuery] int value)
            {
            }
        }

        private sealed class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            public TestModelBinderProviderContext(ModelMetadata metadata, BindingInfo bindingInfo, IModelMetadataProvider metadataProvider)
            {
                Metadata = metadata;
                BindingInfo = bindingInfo;
                MetadataProvider = metadataProvider;
            }

            public override BindingInfo BindingInfo { get; }

            public override ModelMetadata Metadata { get; }

            public override IModelMetadataProvider MetadataProvider { get; }

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                throw new NotSupportedException();
            }
        }
    }
}
