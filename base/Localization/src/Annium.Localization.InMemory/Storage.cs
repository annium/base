using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Localization.Abstractions;

namespace Annium.Localization.InMemory;

/// <summary>
/// In-memory implementation of locale storage
/// </summary>
internal class Storage : ILocaleStorage
{
    /// <summary>
    /// Dictionary of locales by culture
    /// </summary>
    private readonly IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> _locales;

    /// <summary>
    /// Initializes a new instance of the Storage class
    /// </summary>
    /// <param name="locales">The locales dictionary</param>
    public Storage(IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> locales)
    {
        _locales = locales;
    }

    /// <summary>
    /// Loads locale data for the specified target type and culture
    /// </summary>
    /// <param name="target">The target type to load locale for</param>
    /// <param name="culture">The culture to load</param>
    /// <returns>A dictionary of localization entries</returns>
    public IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture)
    {
        return _locales.TryGetValue(culture, out var locale) ? locale : new Dictionary<string, string>();
    }
}
