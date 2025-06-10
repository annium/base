using System;
using System.Linq;
using Annium.Configuration.Abstractions;
using Annium.Configuration.CommandLine.Internal;

namespace Annium.Configuration.CommandLine;

/// <summary>
/// Extension methods for IConfigurationContainer to add command line arguments
/// </summary>
public static class ConfigurationContainerExtensions
{
    /// <summary>
    /// Adds command line arguments from Environment.GetCommandLineArgs() to the configuration container
    /// </summary>
    /// <param name="container">The configuration container</param>
    /// <returns>The container for method chaining</returns>
    public static TContainer AddCommandLineArgs<TContainer>(this TContainer container)
        where TContainer : IConfigurationContainer
    {
        return container.AddCommandLineArgs(Environment.GetCommandLineArgs().Skip(1).ToArray());
    }

    /// <summary>
    /// Adds the specified command line arguments to the configuration container
    /// </summary>
    /// <param name="container">The configuration container</param>
    /// <param name="args">Command line arguments to add</param>
    /// <returns>The container for method chaining</returns>
    public static TContainer AddCommandLineArgs<TContainer>(this TContainer container, string[] args)
        where TContainer : IConfigurationContainer
    {
        var configuration = new CommandLineConfigurationProvider(args).Read();

        container.Add(configuration);

        return container;
    }
}
