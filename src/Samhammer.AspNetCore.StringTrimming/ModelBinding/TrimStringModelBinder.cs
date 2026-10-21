using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Samhammer.AspNetCore.StringTrimming.ModelBinding
{
    /// <summary>
    /// Binds string values from route, query and form and trims leading and trailing whitespace.
    /// Like the default binder, empty or whitespace-only values become null if <see cref="ModelMetadata.ConvertEmptyStringToNull"/> is set.
    /// </summary>
    public class TrimStringModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var value = valueProviderResult.FirstValue?.Trim();
            if (string.IsNullOrEmpty(value) && bindingContext.ModelMetadata.ConvertEmptyStringToNull)
            {
                value = null;
            }

            bindingContext.Result = ModelBindingResult.Success(value);

            return Task.CompletedTask;
        }
    }
}
