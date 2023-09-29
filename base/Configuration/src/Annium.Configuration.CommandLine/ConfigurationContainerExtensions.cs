using System;
using System.Linq;
using Annium.Configuration.CommandLine.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Configuration.Abstractions;

public static class ConfigurationContainerExtensions
{
    public static TContainer AddCommandLineArgs<TContainer>(
        this TContainer container
    )
        where TContainer : IConfigurationContainer
    {
        return container.AddCommandLineArgs(Environment.GetCommandLineArgs().Skip(1).ToArray());
    }

    public static TContainer AddCommandLineArgs<TContainer>(
        this TContainer container,
        string[] args
    )
        where TContainer : IConfigurationContainer
    {
        var configuration = new CommandLineConfigurationProvider(args).Read();

        container.Add(configuration);

        return container;
    }
}