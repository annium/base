using System.Collections.Generic;
using System.Globalization;
using Annium.Localization.Abstractions;

namespace Annium.Localization.InMemory;

/// <summary>
/// Extension methods for configuring in-memory localization storage
/// </summary>
public static class LocalizationOptionsExtensions
{
    /// <summary>
    /// Configures the localization to use in-memory storage with empty locales
    /// </summary>
    /// <param name="options">The localization options</param>
    /// <returns>The options instance for method chaining</returns>
    public static LocalizationOptions UseInMemoryStorage(this LocalizationOptions options)
    {
        return options.UseInMemoryStorage(new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>());
    }

    /// <summary>
    /// Configures the localization to use in-memory storage with provided locales
    /// </summary>
    /// <param name="options">The localization options</param>
    /// <param name="locales">The locales dictionary</param>
    /// <returns>The options instance for method chaining</returns>
    public static LocalizationOptions UseInMemoryStorage(
        this LocalizationOptions options,
        IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> locales
    )
    {
        options.SetLocaleStorage(container =>
        {
            var storage = new Storage(locales);
            container.Add<ILocaleStorage>(storage).AsSelf().Singleton();
        });

        return options;
    }
}
