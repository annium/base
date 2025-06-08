using System;
using System.Collections.Generic;
using System.Globalization;

namespace Annium.Localization.Abstractions;

/// <summary>
/// Internal implementation of type-specific localizer
/// </summary>
/// <typeparam name="T">The type to localize for</typeparam>
internal class Localizer<T> : ILocalizer<T>
{
    /// <summary>
    /// Cache of loaded locales by culture
    /// </summary>
    private readonly IDictionary<CultureInfo, IReadOnlyDictionary<string, string>> _locales =
        new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();

    /// <summary>
    /// The locale storage provider
    /// </summary>
    private readonly ILocaleStorage _storage;

    /// <summary>
    /// Function to get the current culture
    /// </summary>
    private readonly Func<CultureInfo> _getCulture;

    /// <summary>
    /// Initializes a new instance of the Localizer class
    /// </summary>
    /// <param name="storage">The locale storage provider</param>
    /// <param name="getCulture">Function to get the current culture</param>
    public Localizer(ILocaleStorage storage, Func<CultureInfo> getCulture)
    {
        _storage = storage;
        _getCulture = getCulture;
    }

    /// <summary>
    /// Gets the localized string for the specified entry
    /// </summary>
    /// <param name="entry">The entry key</param>
    /// <returns>The localized string</returns>
    public string this[string entry] => Translate(entry);

    /// <summary>
    /// Gets the localized string for the specified entry with formatting arguments
    /// </summary>
    /// <param name="entry">The entry key</param>
    /// <param name="arguments">The formatting arguments</param>
    /// <returns>The formatted localized string</returns>
    public string this[string entry, params object[] arguments] =>
        string.Format(_getCulture(), Translate(entry), arguments);

    /// <summary>
    /// Gets the localized string for the specified entry with formatting arguments
    /// </summary>
    /// <param name="entry">The entry key</param>
    /// <param name="arguments">The formatting arguments</param>
    /// <returns>The formatted localized string</returns>
    public string this[string entry, IEnumerable<object> arguments] =>
        string.Format(_getCulture(), Translate(entry), arguments);

    /// <summary>
    /// Translates the specified entry to the current culture
    /// </summary>
    /// <param name="entry">The entry key to translate</param>
    /// <returns>The translated string or the original entry if no translation found</returns>
    private string Translate(string entry)
    {
        var culture = _getCulture();
        var locale = ResolveLocale(culture);

        return locale.TryGetValue(entry, out var translation) ? translation : entry;
    }

    /// <summary>
    /// Resolves and caches the locale dictionary for the specified culture
    /// </summary>
    /// <param name="culture">The culture to resolve locale for</param>
    /// <returns>The locale dictionary for the specified culture</returns>
    private IReadOnlyDictionary<string, string> ResolveLocale(CultureInfo culture)
    {
        lock (_locales)
        {
            if (_locales.TryGetValue(culture, out var locale))
                return locale;

            return _locales[culture] = _storage.LoadLocale(typeof(T), culture);
        }
    }
}
