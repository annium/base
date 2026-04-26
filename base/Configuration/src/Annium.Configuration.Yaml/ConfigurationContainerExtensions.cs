using System;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Yaml.Internal;

namespace Annium.Configuration.Yaml;

/// <summary>
/// Extension methods for <see cref="IConfigurationContainer"/> to register deferred YAML sources.
/// </summary>
public static class ConfigurationContainerExtensions
{
    /// <summary>
    /// Registers a deferred YAML file source. The file is read by
    /// <see cref="Abstractions.ConfigurationContainerExtensions.BuildAsync"/>; missing or
    /// unreadable files throw unless <paramref name="optional"/> is true.
    /// </summary>
    /// <typeparam name="TContainer">Container type</typeparam>
    /// <param name="container">The configuration container</param>
    /// <param name="path">Path to the YAML file</param>
    /// <param name="optional">Whether load failures are silenced</param>
    /// <returns>The container for method chaining</returns>
    public static TContainer AddYamlFile<TContainer>(this TContainer container, string path, bool optional = false)
        where TContainer : IConfigurationContainer
    {
        container.AddSource(new YamlFileSource(path, optional));
        return container;
    }

    /// <summary>
    /// Registers a deferred remote YAML source. The HTTP call is made by
    /// <see cref="Abstractions.ConfigurationContainerExtensions.BuildAsync"/>; non-2xx responses,
    /// network errors, and timeouts throw unless <paramref name="optional"/> is true.
    /// </summary>
    /// <typeparam name="TContainer">Container type</typeparam>
    /// <param name="container">The configuration container</param>
    /// <param name="uri">URI to fetch</param>
    /// <param name="optional">Whether fetch failures are silenced</param>
    /// <param name="timeout">Per-source timeout (default: 30 seconds)</param>
    /// <returns>The container for method chaining</returns>
    public static TContainer AddRemoteYaml<TContainer>(
        this TContainer container,
        Uri uri,
        bool optional = false,
        TimeSpan? timeout = null
    )
        where TContainer : IConfigurationContainer
    {
        container.AddSource(new YamlRemoteSource(uri, optional, timeout));
        return container;
    }
}
