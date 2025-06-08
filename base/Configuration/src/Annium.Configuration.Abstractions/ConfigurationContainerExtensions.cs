using Annium.Configuration.Abstractions.Internal;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Extension methods for IConfigurationContainer to add configuration data
/// </summary>
public static class ConfigurationContainerExtensions
{
    /// <summary>
    /// Adds configuration data from an object to the container
    /// </summary>
    /// <param name="container">The configuration container</param>
    /// <param name="config">The configuration object to add</param>
    /// <returns>The container for method chaining</returns>
    public static TContainer Add<TContainer>(this TContainer container, object? config)
        where TContainer : IConfigurationContainer
    {
        var configuration = new ObjectConfigurationProvider(config).Read();

        container.Add(configuration);

        return container;
    }
}
