using System.Collections.Generic;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Interface for building configuration objects from pre-merged configuration data
/// </summary>
public interface IConfigurationBuilder
{
    /// <summary>
    /// Adds pre-merged configuration data to be processed by <see cref="Build{T}"/>.
    /// </summary>
    /// <param name="config">Configuration data to add</param>
    void Add(IReadOnlyDictionary<string[], string> config);

    /// <summary>
    /// Builds an instance of type T from the configuration data
    /// </summary>
    /// <typeparam name="T">The configuration type to build.</typeparam>
    /// <returns>Configured instance of type T</returns>
    T Build<T>()
        where T : new();
}
