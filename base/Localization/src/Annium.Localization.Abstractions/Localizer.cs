using System;
using System.Collections.Generic;
using System.Globalization;

namespace Annium.Localization.Abstractions;

internal class Localizer<T> : ILocalizer<T>
{
    private readonly IDictionary<CultureInfo, IReadOnlyDictionary<string, string>> _locales =
        new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();

    private readonly ILocaleStorage _storage;
    private readonly Func<CultureInfo> _getCulture;

    public Localizer(
        ILocaleStorage storage,
        Func<CultureInfo> getCulture
    )
    {
        _storage = storage;
        _getCulture = getCulture;
    }

    public string this[string entry] => Translate(entry);

    public string this[string entry, params object[] arguments] => string.Format(_getCulture(), Translate(entry), arguments);

    public string this[string entry, IEnumerable<object> arguments] => string.Format(_getCulture(), Translate(entry), arguments);

    private string Translate(string entry)
    {
        var culture = _getCulture();
        var locale = ResolveLocale(culture);

        return locale.TryGetValue(entry, out var translation) ? translation : entry;
    }

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