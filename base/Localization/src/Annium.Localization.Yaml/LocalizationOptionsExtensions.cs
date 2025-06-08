using Annium.Localization.Abstractions;
using Annium.Localization.Yaml;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring YAML-based localization storage
/// </summary>
public static class LocalizationOptionsExtensions
{
    /// <summary>
    /// Configures the localization to use YAML file storage
    /// </summary>
    /// <param name="options">The localization options</param>
    /// <returns>The options instance for method chaining</returns>
    public static LocalizationOptions UseYamlStorage(this LocalizationOptions options)
    {
        options.SetLocaleStorage(container =>
        {
            container.Add<ILocaleStorage, Storage>().Singleton();
        });

        return options;
    }
}
