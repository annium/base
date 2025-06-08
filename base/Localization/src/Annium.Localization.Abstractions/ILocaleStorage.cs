using System;
using System.Collections.Generic;
using System.Globalization;

namespace Annium.Localization.Abstractions;

/// <summary>
/// Interface for loading locale data
/// </summary>
public interface ILocaleStorage
{
    /// <summary>
    /// Loads locale data for the specified target type and culture
    /// </summary>
    /// <param name="target">The target type to load locale for</param>
    /// <param name="culture">The culture to load</param>
    /// <returns>A dictionary of localization entries</returns>
    IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture);
}
