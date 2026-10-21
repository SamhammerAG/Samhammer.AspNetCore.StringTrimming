using System.Globalization;
using System.Reflection;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.Test
{
    /// <summary>
    /// Binds action parameters with the binders MVC creates (incl. item binders of collections), like in a real request.
    /// </summary>
    public sealed class CollectionModelBindingTest : IDisposable
    {
        private const string Query = "?value=%20a%20&value=b%20&value=%09c";

        private readonly ServiceProvider services;

        public CollectionModelBindingTest()
        {
            var serviceCollection = new ServiceCollection().AddLogging();
            serviceCollection.AddControllers().AddStringTrimmingModelBinder();
            services = serviceCollection.BuildServiceProvider();
        }

        public void Dispose()
        {
            services.Dispose();
        }

        [Theory]
        [InlineData(nameof(TestController.List))]
        [InlineData(nameof(TestController.Array))]
        [InlineData(nameof(TestController.Enumerable))]
        [InlineData(nameof(TestController.ReadOnlyList))]
        [InlineData(nameof(TestController.HashSet))]
        public async Task StringCollection_ItemsAreTrimmed(string methodName)
        {
            var result = await Bind(methodName, Query);

            result.IsModelSet.Should().BeTrue();
            ((IEnumerable<string>)result.Model).Should().BeEquivalentTo("a", "b", "c");
        }

        [Theory]
        [InlineData(nameof(TestController.NoTrimList))]
        [InlineData(nameof(TestController.NoTrimArray))]
        [InlineData(nameof(TestController.NoTrimEnumerable))]
        [InlineData(nameof(TestController.NoTrimReadOnlyList))]
        [InlineData(nameof(TestController.NoTrimHashSet))]
        [InlineData(nameof(TestController.NoTrimListWithoutSource))]
        public async Task StringCollectionWithNoTrim_KeepsWhitespace(string methodName)
        {
            var result = await Bind(methodName, Query);

            result.IsModelSet.Should().BeTrue();
            ((IEnumerable<string>)result.Model).Should().BeEquivalentTo(" a ", "b ", "\tc");
        }

        [Fact]
        public async Task StringCollectionWithNoTrim_ReturnsRequestedType()
        {
            (await Bind(nameof(TestController.NoTrimList), Query)).Model.Should().BeOfType<List<string>>();
            (await Bind(nameof(TestController.NoTrimArray), Query)).Model.Should().BeOfType<string[]>();
            (await Bind(nameof(TestController.NoTrimHashSet), Query)).Model.Should().BeOfType<HashSet<string>>();
        }

        [Fact]
        public async Task String_IsTrimmed_AndWithNoTrim_KeepsWhitespace()
        {
            (await Bind(nameof(TestController.Single), "?value=%20a%20")).Model.Should().Be("a");
            (await Bind(nameof(TestController.NoTrimSingle), "?value=%20a%20")).Model.Should().Be(" a ");
        }

        [Fact]
        public async Task ComplexModel_CollectionPropertyWithNoTrim_KeepsWhitespace()
        {
            var result = await Bind(nameof(TestController.Model), "?tags=%20a%20&rawTags=%20b%20&name=%20c%20&rawName=%20d%20");

            var model = result.Model.Should().BeOfType<QueryModel>().Subject;
            model.Tags.Should().Equal("a");
            model.RawTags.Should().Equal(" b ");
            model.Name.Should().Be("c");
            model.RawName.Should().Be(" d ");
        }

        private async Task<ModelBindingResult> Bind(string methodName, string query)
        {
            var parameter = typeof(TestController).GetMethod(methodName)!.GetParameters().Single();
            var metadataProvider = (ModelMetadataProvider)services.GetRequiredService<IModelMetadataProvider>();
            var metadata = metadataProvider.GetMetadataForParameter(parameter);
            var bindingInfo = BindingInfo.GetBindingInfo(parameter.GetCustomAttributes(), metadata);

            var binder = services.GetRequiredService<IModelBinderFactory>().CreateBinder(new ModelBinderFactoryContext
            {
                Metadata = metadata,
                BindingInfo = bindingInfo,
                CacheToken = parameter,
            });

            var httpContext = new DefaultHttpContext { RequestServices = services };
            httpContext.Request.QueryString = new QueryString(query);
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var valueProvider = new QueryStringValueProvider(BindingSource.Query, httpContext.Request.Query, CultureInfo.InvariantCulture);
            var parameterDescriptor = new ParameterDescriptor { Name = parameter.Name, ParameterType = parameter.ParameterType, BindingInfo = bindingInfo };

            return await services.GetRequiredService<ParameterBinder>()
                .BindModelAsync(actionContext, binder, valueProvider, parameterDescriptor, metadata, value: null, container: null);
        }

        public class TestController
        {
            public void Single([FromQuery] string value)
            {
            }

            public void NoTrimSingle([FromQuery][NoTrim] string value)
            {
            }

            public void List([FromQuery] List<string> value)
            {
            }

            public void Array([FromQuery] string[] value)
            {
            }

            public void Enumerable([FromQuery] IEnumerable<string> value)
            {
            }

            public void ReadOnlyList([FromQuery] IReadOnlyList<string> value)
            {
            }

            public void HashSet([FromQuery] HashSet<string> value)
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

            public void Model([FromQuery] QueryModel value)
            {
            }
        }

        public class QueryModel
        {
            public List<string> Tags { get; set; }

            [NoTrim]
            public List<string> RawTags { get; set; }

            public string Name { get; set; }

            [NoTrim]
            public string RawName { get; set; }
        }
    }
}
