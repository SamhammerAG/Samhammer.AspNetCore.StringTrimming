using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Samhammer.AspNetCore.StringTrimming.ModelBinding
{
    /// <summary>
    /// Provides the <see cref="TrimStringModelBinder"/> for string values from route, query and form.
    /// JSON bodies are trimmed by the serializer integration. Header, services and custom binders keep their default binder.
    /// </summary>
    /// <remarks>
    /// Items of string collections are trimmed as well, because MVC creates their binders with the metadata of string.
    /// That metadata does not contain the <see cref="NoTrimAttribute"/> of the collection, so for string collections marked with
    /// <see cref="NoTrimAttribute"/> the default collection binder is created here with an untrimmed item binder.
    /// </remarks>
    public class TrimStringModelBinderProvider : IModelBinderProvider
    {
        private static readonly IModelBinder Binder = new TrimStringModelBinder();

        private static readonly BindingSource[] SupportedBindingSources =
        {
            BindingSource.Path,
            BindingSource.Query,
            BindingSource.Form,
            BindingSource.ModelBinding,
        };

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.BindingInfo.BinderType != null)
            {
                return null;
            }

            var bindingSource = context.BindingInfo.BindingSource;
            if (bindingSource != null && !SupportedBindingSources.Contains(bindingSource))
            {
                return null;
            }

            var modelType = context.Metadata.ModelType;

            if (modelType == typeof(string))
            {
                return HasNoTrimAttribute(context.Metadata) ? null : Binder;
            }

            if (IsStringCollection(modelType) && HasNoTrimAttribute(context.Metadata))
            {
                return CreateUntrimmedCollectionBinder(context);
            }

            return null;
        }

        // Same types as the default CollectionModelBinderProvider handles: arrays, ICollection<string> and interfaces implemented by List<string>
        private static bool IsStringCollection(Type modelType)
        {
            if (modelType == typeof(string[]))
            {
                return true;
            }

            if (modelType.IsArray || !typeof(IEnumerable<string>).IsAssignableFrom(modelType))
            {
                return false;
            }

            return typeof(ICollection<string>).IsAssignableFrom(modelType) || modelType.IsAssignableFrom(typeof(List<string>));
        }

        private static IModelBinder CreateUntrimmedCollectionBinder(ModelBinderProviderContext context)
        {
            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            var mvcOptions = context.Services.GetRequiredService<IOptions<MvcOptions>>().Value;
            var elementBinder = new SimpleTypeModelBinder(typeof(string), loggerFactory);

            return context.Metadata.ModelType.IsArray
                ? new ArrayModelBinder<string>(elementBinder, loggerFactory, allowValidatingTopLevelNodes: true, mvcOptions)
                : new CollectionModelBinder<string>(elementBinder, loggerFactory, allowValidatingTopLevelNodes: true, mvcOptions);
        }

        private static bool HasNoTrimAttribute(ModelMetadata metadata)
        {
            return metadata is DefaultModelMetadata defaultMetadata
                && defaultMetadata.Attributes.Attributes.OfType<NoTrimAttribute>().Any();
        }
    }
}
