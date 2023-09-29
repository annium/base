using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Localization.Abstractions;

namespace Annium.Localization.InMemory;

internal class Storage : ILocaleStorage
{
    private readonly IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> _locales;

    public Storage(
        IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> locales
    )
    {
        _locales = locales;
    }

    public IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture)
    {
        return _locales.TryGetValue(culture, out var locale) ? locale : new Dictionary<string, string>();
    }
}