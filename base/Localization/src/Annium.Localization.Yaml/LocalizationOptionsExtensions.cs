using Annium.Localization.Abstractions;
using Annium.Localization.Yaml;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LocalizationOptionsExtensions
{
    public static LocalizationOptions UseYamlStorage(
        this LocalizationOptions options
    )
    {
        options.SetLocaleStorage(container => { container.Add<ILocaleStorage, Storage>().Singleton(); });

        return options;
    }
}