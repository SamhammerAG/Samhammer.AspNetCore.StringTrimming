using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Samhammer.AspNetCore.StringTrimming.ModelBinding
{
    /// <summary>
    /// Provides the <see cref="TrimStringModelBinder"/> for string values from route, query and form.
    /// JSON bodies are trimmed by the serializer integration. Header, services and custom binders keep their default binder.
    /// </summary>
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

            if (context.Metadata.ModelType != typeof(string) || context.BindingInfo.BinderType != null)
            {
                return null;
            }

            var bindingSource = context.BindingInfo.BindingSource;
            if (bindingSource != null && !SupportedBindingSources.Contains(bindingSource))
            {
                return null;
            }

            return HasNoTrimAttribute(context.Metadata) ? null : Binder;
        }

        private static bool HasNoTrimAttribute(ModelMetadata metadata)
        {
            return metadata is DefaultModelMetadata defaultMetadata
                && defaultMetadata.Attributes.Attributes.OfType<NoTrimAttribute>().Any();
        }
    }
}
