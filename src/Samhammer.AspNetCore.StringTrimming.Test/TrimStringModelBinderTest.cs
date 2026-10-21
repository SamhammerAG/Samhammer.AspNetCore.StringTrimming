using System.Globalization;
using AwesomeAssertions;
using Samhammer.AspNetCore.StringTrimming.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.Test
{
    public class TrimStringModelBinderTest
    {
        private const string ModelName = "input";

        private readonly TrimStringModelBinder binder = new();

        [Fact]
        public async Task BindModelAsync_WithFormValue_TrimsValue()
        {
            var valueProvider = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(new Dictionary<string, StringValues> { { ModelName, "  form value  " } }),
                CultureInfo.InvariantCulture);

            var result = await Bind(valueProvider);

            result.IsModelSet.Should().BeTrue();
            result.Model.Should().Be("form value");
        }

        [Fact]
        public async Task BindModelAsync_WithQueryValue_TrimsValue()
        {
            var valueProvider = new QueryStringValueProvider(
                BindingSource.Query,
                new QueryCollection(new Dictionary<string, StringValues> { { ModelName, "\t query value \r\n" } }),
                CultureInfo.InvariantCulture);

            var result = await Bind(valueProvider);

            result.Model.Should().Be("query value");
        }

        [Fact]
        public async Task BindModelAsync_WithRouteValue_TrimsValue()
        {
            var valueProvider = new RouteValueProvider(
                BindingSource.Path,
                new RouteValueDictionary { { ModelName, "  route value  " } },
                CultureInfo.InvariantCulture);

            var result = await Bind(valueProvider);

            result.Model.Should().Be("route value");
        }

        [Theory]
        [InlineData("", true, null)]
        [InlineData("   ", true, null)]
        [InlineData("   ", false, "")]
        public async Task BindModelAsync_WithEmptyValue_RespectsConvertEmptyStringToNull(string value, bool convertEmptyStringToNull, string expected)
        {
            var valueProvider = new QueryStringValueProvider(
                BindingSource.Query,
                new QueryCollection(new Dictionary<string, StringValues> { { ModelName, value } }),
                CultureInfo.InvariantCulture);

            var result = await Bind(valueProvider, convertEmptyStringToNull);

            result.IsModelSet.Should().BeTrue();
            result.Model.Should().Be(expected);
        }

        [Fact]
        public async Task BindModelAsync_WithoutValue_DoesNotSetModel()
        {
            var valueProvider = new QueryStringValueProvider(BindingSource.Query, new QueryCollection(), CultureInfo.InvariantCulture);

            var result = await Bind(valueProvider);

            result.IsModelSet.Should().BeFalse();
        }

        private async Task<ModelBindingResult> Bind(IValueProvider valueProvider, bool convertEmptyStringToNull = true)
        {
            var detailsProvider = new ConvertEmptyStringToNullProvider(convertEmptyStringToNull);
            var metadataProvider = new DefaultModelMetadataProvider(detailsProvider);
            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(typeof(string)),
                ModelName = ModelName,
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider
            };

            await binder.BindModelAsync(bindingContext);

            return bindingContext.Result;
        }

        // Minimal metadata details provider (the framework's composite provider is internal)
        private sealed class ConvertEmptyStringToNullProvider : ICompositeMetadataDetailsProvider
        {
            private readonly bool convertEmptyStringToNull;

            public ConvertEmptyStringToNullProvider(bool convertEmptyStringToNull)
            {
                this.convertEmptyStringToNull = convertEmptyStringToNull;
            }

            public void CreateBindingMetadata(BindingMetadataProviderContext context)
            {
            }

            public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
            {
                context.DisplayMetadata.ConvertEmptyStringToNull = convertEmptyStringToNull;
            }

            public void CreateValidationMetadata(ValidationMetadataProviderContext context)
            {
            }
        }
    }
}
