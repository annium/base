using System;
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
    /// An empty per-type locales map.
    /// </summary>
    private static readonly IReadOnlyDictionary<
        Type,
        IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>
    > _noTypeLocales = new Dictionary<Type, IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>>();

    /// <summary>
    /// An empty shared locales map.
    /// </summary>
    private static readonly IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> _noSharedLocales =
        new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();

    /// <summary>
    /// Configures the localization to use in-memory storage with empty locales
    /// </summary>
    /// <param name="options">The localization options</param>
    /// <returns>The options instance for method chaining</returns>
    public static LocalizationOptions UseInMemoryStorage(this LocalizationOptions options)
    {
        return options.UseInMemoryStorage(_noSharedLocales);
    }

    /// <summary>
    /// Configures the localization to use in-memory storage with shared locales applied to every type
    /// </summary>
    /// <param name="options">The localization options</param>
    /// <param name="locales">The shared locales dictionary, keyed by culture</param>
    /// <returns>The options instance for method chaining</returns>
    public static LocalizationOptions UseInMemoryStorage(
        this LocalizationOptions options,
        IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> locales
    )
    {
        return options.UseInMemoryStorage(_noTypeLocales, locales);
    }

    /// <summary>
    /// Configures the localization to use in-memory storage with per-type locales
    /// </summary>
    /// <param name="options">The localization options</param>
    /// <param name="localesByType">Per-type locales, keyed by target type then culture</param>
    /// <returns>The options instance for method chaining</returns>
    public static LocalizationOptions UseInMemoryStorage(
        this LocalizationOptions options,
        IReadOnlyDictionary<Type, IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>> localesByType
    )
    {
        return options.UseInMemoryStorage(localesByType, _noSharedLocales);
    }

    /// <summary>
    /// Configures the localization to use in-memory storage with both per-type locales and shared
    /// locales; per-type entries take precedence, shared entries apply to any type without them.
    /// </summary>
    /// <param name="options">The localization options</param>
    /// <param name="localesByType">Per-type locales, keyed by target type then culture</param>
    /// <param name="sharedLocales">Shared locales applied to types without type-specific entries</param>
    /// <returns>The options instance for method chaining</returns>
    public static LocalizationOptions UseInMemoryStorage(
        this LocalizationOptions options,
        IReadOnlyDictionary<Type, IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>> localesByType,
        IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> sharedLocales
    )
    {
        options.SetLocaleStorage(container =>
        {
            var storage = new Storage(localesByType, sharedLocales);
            container.Add<ILocaleStorage>(storage).AsSelf().Singleton();
        });

        return options;
    }
}
