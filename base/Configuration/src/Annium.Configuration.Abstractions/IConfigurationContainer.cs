using System.Collections.Generic;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Interface for storing and retrieving configuration data
/// </summary>
public interface IConfigurationContainer
{
    /// <summary>
    /// Adds configuration data to the container
    /// </summary>
    /// <param name="config">Configuration data to add</param>
    /// <returns>The container for method chaining</returns>
    IConfigurationContainer Add(IReadOnlyDictionary<string[], string> config);

    /// <summary>
    /// Gets all configuration data from the container
    /// </summary>
    /// <returns>Dictionary containing all configuration data</returns>
    IReadOnlyDictionary<string[], string> Get();
}
