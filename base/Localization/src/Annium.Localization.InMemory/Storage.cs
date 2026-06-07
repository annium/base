using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Localization.Abstractions;

namespace Annium.Localization.InMemory;

/// <summary>
/// In-memory implementation of locale storage. Locales may be registered per target type,
/// or as a shared set applied to any type that has no type-specific locale.
/// </summary>
internal class Storage : ILocaleStorage
{
    /// <summary>
    /// Per-type locales: target type → culture → entries.
    /// </summary>
    private readonly IReadOnlyDictionary<
        Type,
        IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>
    > _localesByType;

    /// <summary>
    /// Shared locales applied to any target type that has no type-specific entries.
    /// </summary>
    private readonly IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> _sharedLocales;

    /// <summary>
    /// Shared empty locale returned when no entries exist for a (target, culture) pair.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> _empty = new Dictionary<string, string>();

    /// <summary>
    /// Initializes a new instance of the Storage class.
    /// </summary>
    /// <param name="localesByType">Per-type locales keyed by target type then culture.</param>
    /// <param name="sharedLocales">Shared locales applied to types without type-specific entries.</param>
    public Storage(
        IReadOnlyDictionary<Type, IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>>> localesByType,
        IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> sharedLocales
    )
    {
        _localesByType = localesByType;
        _sharedLocales = sharedLocales;
    }

    /// <summary>
    /// Loads locale data for the specified target type and culture. Type-specific entries take
    /// precedence; otherwise the shared locales for the culture are used.
    /// </summary>
    /// <param name="target">The target type to load locale for</param>
    /// <param name="culture">The culture to load</param>
    /// <returns>A dictionary of localization entries</returns>
    public IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture)
    {
        if (_localesByType.TryGetValue(target, out var byCulture) && byCulture.TryGetValue(culture, out var typed))
            return typed;

        return _sharedLocales.TryGetValue(culture, out var shared) ? shared : _empty;
    }
}
