using Annium.Configuration.Abstractions.Internal;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Factory methods for the configuration system. Provides a public path to obtain an empty
/// <see cref="IConfigurationContainer"/> without referencing internal implementation types.
/// </summary>
public static class ConfigurationFactory
{
    /// <summary>
    /// Creates an empty <see cref="IConfigurationContainer"/>.
    /// </summary>
    /// <returns>A new empty configuration container.</returns>
    public static IConfigurationContainer CreateContainer() => new ConfigurationContainer();
}
