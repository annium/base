using System.Collections.Generic;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Interface for storing and retrieving configuration data. Holds two layers: directly-merged
/// flat data (via <see cref="Add"/>) and deferred <see cref="IConfigurationSource"/> registrations
/// (via <see cref="AddSource"/>). Deferred sources are loaded by
/// <see cref="ConfigurationContainerExtensions.BuildAsync"/>.
/// </summary>
public interface IConfigurationContainer
{
    /// <summary>
    /// Sources registered for deferred loading, in registration order.
    /// </summary>
    IReadOnlyList<IConfigurationSource> Sources { get; }

    /// <summary>
    /// Registers a deferred configuration source. The source's <c>LoadAsync</c> is called by
    /// <see cref="ConfigurationContainerExtensions.BuildAsync"/>; the result is merged into
    /// this container in registration order.
    /// </summary>
    /// <param name="source">Source to register</param>
    void AddSource(IConfigurationSource source);

    /// <summary>
    /// Adds configuration data to the container
    /// </summary>
    /// <param name="config">Configuration data to add</param>
    void Add(IReadOnlyDictionary<string[], string> config);

    /// <summary>
    /// Gets all configuration data from the container
    /// </summary>
    /// <returns>Dictionary containing all configuration data</returns>
    IReadOnlyDictionary<string[], string> Get();
}
