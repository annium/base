using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Annium.Localization.Abstractions;

/// <summary>
/// Internal implementation of type-specific localizer
/// </summary>
/// <typeparam name="T">The type to localize for</typeparam>
internal class Localizer<T> : ILocalizer<T>
{
    /// <summary>
    /// Cache of loaded locales by culture. Wrapping each value in a <see cref="Lazy{T}"/> lets the
    /// cache be lock-free while still loading each culture's locale exactly once, even under concurrent
    /// first access for distinct cultures.
    /// </summary>
    private readonly ConcurrentDictionary<CultureInfo, Lazy<IReadOnlyDictionary<string, string>>> _locales = new();

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
    public string this[string entry] => Translate(entry, _getCulture());

    /// <summary>
    /// Gets the localized string for the specified entry with formatting arguments
    /// </summary>
    /// <param name="entry">The entry key</param>
    /// <param name="arguments">The formatting arguments</param>
    /// <returns>The formatted localized string</returns>
    public string this[string entry, params object[] arguments] => Format(entry, arguments);

    /// <summary>
    /// Gets the localized string for the specified entry with formatting arguments
    /// </summary>
    /// <param name="entry">The entry key</param>
    /// <param name="arguments">The formatting arguments</param>
    /// <returns>The formatted localized string</returns>
    public string this[string entry, IEnumerable<object> arguments] => Format(entry, arguments.ToArray());

    /// <summary>
    /// Formats the translation for the specified entry under the current culture, resolving the
    /// culture once so the template and the format provider always agree.
    /// </summary>
    /// <param name="entry">The entry key</param>
    /// <param name="arguments">The formatting arguments</param>
    /// <returns>The formatted localized string</returns>
    private string Format(string entry, object[] arguments)
    {
        var culture = _getCulture();
        return string.Format(culture, Translate(entry, culture), arguments);
    }

    /// <summary>
    /// Translates the specified entry using the given culture
    /// </summary>
    /// <param name="entry">The entry key to translate</param>
    /// <param name="culture">The culture to translate for</param>
    /// <returns>The translated string or the original entry if no translation found</returns>
    private string Translate(string entry, CultureInfo culture)
    {
        var locale = ResolveLocale(culture);

        // a present-but-null entry (e.g. a YAML key with no value) is treated as a miss → return the key
        return locale.TryGetValue(entry, out var translation) && translation is not null ? translation : entry;
    }

    /// <summary>
    /// Resolves and caches the locale dictionary for the specified culture
    /// </summary>
    /// <param name="culture">The culture to resolve locale for</param>
    /// <returns>The locale dictionary for the specified culture</returns>
    private IReadOnlyDictionary<string, string> ResolveLocale(CultureInfo culture) =>
        _locales
            .GetOrAdd(
                culture,
                c => new Lazy<IReadOnlyDictionary<string, string>>(() => _storage.LoadLocale(typeof(T), c))
            )
            .Value;
}
