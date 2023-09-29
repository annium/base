using System;
using System.Collections.Generic;
using System.Globalization;

namespace Annium.Localization.Abstractions;

public interface ILocaleStorage
{
    IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture);
}