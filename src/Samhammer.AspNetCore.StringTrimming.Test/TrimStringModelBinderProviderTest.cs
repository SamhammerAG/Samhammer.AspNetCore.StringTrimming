using System.Reflection;
using AwesomeAssertions;
using Samhammer.AspNetCore.StringTrimming.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.Test
{
    public class TrimStringModelBinderProviderTest
    {
        private readonly TrimStringModelBinderProvider provider = new();

        private readonly EmptyModelMetadataProvider metadataProvider = new();

        private readonly IServiceProvider services = new ServiceCollection().AddLogging().AddOptions().BuildServiceProvider();

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
        [InlineData(nameof(TestController.List))]
        [InlineData(nameof(TestController.BodyNoTrimList))]
        [InlineData(nameof(TestController.NoTrimComplexType))]
        public void GetBinder_ForUnsupportedParameter_ReturnsNull(string methodName)
        {
            var binder = provider.GetBinder(CreateContext(methodName));

            binder.Should().BeNull();
        }

        [Theory]
        [InlineData(nameof(QueryModel.Value))]
        [InlineData(nameof(QueryModel.QueryValue))]
        public void GetBinder_ForBoundStringProperty_ReturnsTrimBinder(string propertyName)
        {
            var binder = provider.GetBinder(CreatePropertyContext(propertyName));

            binder.Should().BeOfType<TrimStringModelBinder>();
        }

        [Theory]
        [InlineData(nameof(QueryModel.Raw))]
        [InlineData(nameof(QueryModel.RawQueryValue))]
        [InlineData(nameof(QueryModel.HeaderValue))]
        [InlineData(nameof(QueryModel.Number))]
        public void GetBinder_ForUnsupportedBoundProperty_ReturnsNull(string propertyName)
        {
            var binder = provider.GetBinder(CreatePropertyContext(propertyName));

            binder.Should().BeNull();
        }

        private TestModelBinderProviderContext CreatePropertyContext(string propertyName)
        {
            var property = typeof(QueryModel).GetProperty(propertyName)!;
            var metadata = metadataProvider.GetMetadataForProperty(typeof(QueryModel), propertyName);
            var bindingInfo = BindingInfo.GetBindingInfo(property.GetCustomAttributes(), metadata) ?? new BindingInfo();

            return new TestModelBinderProviderContext(metadata, bindingInfo, metadataProvider, services);
        }

        [Theory]
        [InlineData(nameof(TestController.NoTrimList))]
        [InlineData(nameof(TestController.NoTrimEnumerable))]
        [InlineData(nameof(TestController.NoTrimReadOnlyList))]
        [InlineData(nameof(TestController.NoTrimHashSet))]
        [InlineData(nameof(TestController.NoTrimListWithoutSource))]
        public void GetBinder_ForStringCollectionWithNoTrim_ReturnsCollectionBinder(string methodName)
        {
            var binder = provider.GetBinder(CreateContext(methodName));

            binder.Should().BeOfType<CollectionModelBinder<string>>();
        }

        [Fact]
        public void GetBinder_ForStringArrayWithNoTrim_ReturnsArrayBinder()
        {
            var binder = provider.GetBinder(CreateContext(nameof(TestController.NoTrimArray)));

            binder.Should().BeOfType<ArrayModelBinder<string>>();
        }

        [Fact]
        public void GetBinder_ForBoundStringCollectionPropertyWithNoTrim_ReturnsCollectionBinder()
        {
            var binder = provider.GetBinder(CreatePropertyContext(nameof(QueryModel.RawTags)));

            binder.Should().BeOfType<CollectionModelBinder<string>>();
        }

        private TestModelBinderProviderContext CreateContext(string methodName)
        {
            var parameter = typeof(TestController).GetMethod(methodName)!.GetParameters().Single();
            var metadata = metadataProvider.GetMetadataForParameter(parameter);
            var bindingInfo = BindingInfo.GetBindingInfo(parameter.GetCustomAttributes(), metadata) ?? new BindingInfo();

            return new TestModelBinderProviderContext(metadata, bindingInfo, metadataProvider, services);
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

            public void List([FromQuery] List<string> value)
            {
            }

            public void NoTrimList([FromQuery][NoTrim] List<string> value)
            {
            }

            public void NoTrimArray([FromQuery][NoTrim] string[] value)
            {
            }

            public void NoTrimEnumerable([FromQuery][NoTrim] IEnumerable<string> value)
            {
            }

            public void NoTrimReadOnlyList([FromQuery][NoTrim] IReadOnlyList<string> value)
            {
            }

            public void NoTrimHashSet([FromQuery][NoTrim] HashSet<string> value)
            {
            }

            public void NoTrimListWithoutSource([NoTrim] List<string> value)
            {
            }

            public void BodyNoTrimList([FromBody][NoTrim] List<string> value)
            {
            }

            public void NoTrimComplexType([FromQuery][NoTrim] QueryModel value)
            {
            }
        }

        public class QueryModel
        {
            public string Value { get; set; }

            [FromQuery]
            public string QueryValue { get; set; }

            [NoTrim]
            public string Raw { get; set; }

            [FromQuery]
            [NoTrim]
            public string RawQueryValue { get; set; }

            [FromHeader]
            public string HeaderValue { get; set; }

            public int Number { get; set; }

            [FromQuery]
            [NoTrim]
            public List<string> RawTags { get; set; }
        }

        private sealed class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            public TestModelBinderProviderContext(ModelMetadata metadata, BindingInfo bindingInfo, IModelMetadataProvider metadataProvider, IServiceProvider services)
            {
                Metadata = metadata;
                BindingInfo = bindingInfo;
                MetadataProvider = metadataProvider;
                Services = services;
            }

            public override BindingInfo BindingInfo { get; }

            public override ModelMetadata Metadata { get; }

            public override IModelMetadataProvider MetadataProvider { get; }

            public override IServiceProvider Services { get; }

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                throw new NotSupportedException();
            }
        }
    }
}
