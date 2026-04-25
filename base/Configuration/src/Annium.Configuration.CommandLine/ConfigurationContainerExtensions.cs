using Annium.Configuration.Abstractions;
using Annium.Configuration.CommandLine.Internal;

namespace Annium.Configuration.CommandLine;

/// <summary>
/// Extension methods for <see cref="IConfigurationContainer"/> to register a deferred command-line source.
/// </summary>
public static class ConfigurationContainerExtensions
{
    /// <summary>
    /// Registers a deferred command-line source. When <paramref name="args"/> is null, the
    /// environment's command-line args are read at <see cref="Abstractions.ConfigurationContainerExtensions.BuildAsync"/>
    /// time (skipping the executable path).
    /// </summary>
    /// <typeparam name="TContainer">Container type</typeparam>
    /// <param name="container">The configuration container</param>
    /// <param name="args">Override args, or null to read from the environment at load time</param>
    /// <param name="optional">Whether parse failures are silenced</param>
    /// <returns>The container for method chaining</returns>
    public static TContainer AddCommandLineArgs<TContainer>(
        this TContainer container,
        string[]? args = null,
        bool optional = false
    )
        where TContainer : IConfigurationContainer
    {
        container.AddSource(new CommandLineConfigurationSource(args, optional));
        return container;
    }
}
