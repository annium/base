using System;
using System.Collections.Generic;
using System.Globalization;

namespace Annium.Localization.Abstractions;

/// <summary>
/// Interface for loading locale data
/// </summary>
/// <remarks>
/// The contract is synchronous and load-once-and-cache: implementations resolve a locale on the
/// first request for a given (target, culture) and cache the result, so any blocking I/O (e.g. the
/// YAML backend reading a file) is acceptable only on that first call. Implementations must not
/// perform per-call blocking work or invoke asynchronous APIs on this synchronous path.
/// </remarks>
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
