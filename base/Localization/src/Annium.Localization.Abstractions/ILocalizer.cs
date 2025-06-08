using System.Collections.Generic;

namespace Annium.Localization.Abstractions;

/// <summary>
/// Generic interface for type-specific localization
/// </summary>
/// <typeparam name="T">The type to localize for</typeparam>
public interface ILocalizer<T> : ILocalizer;

/// <summary>
/// Interface for localization services
/// </summary>
public interface ILocalizer
{
    /// <summary>
    /// Gets the localized string for the specified entry
    /// </summary>
    /// <param name="entry">The entry key</param>
    /// <returns>The localized string</returns>
    string this[string entry] { get; }

    /// <summary>
    /// Gets the localized string for the specified entry with formatting arguments
    /// </summary>
    /// <param name="entry">The entry key</param>
    /// <param name="arguments">The formatting arguments</param>
    /// <returns>The formatted localized string</returns>
    string this[string entry, params object[] arguments] { get; }

    /// <summary>
    /// Gets the localized string for the specified entry with formatting arguments
    /// </summary>
    /// <param name="entry">The entry key</param>
    /// <param name="arguments">The formatting arguments</param>
    /// <returns>The formatted localized string</returns>
    string this[string entry, IEnumerable<object> arguments] { get; }
}
