using Annium.Configuration.Abstractions.Internal;

namespace Annium.Configuration.Abstractions;

public static class ConfigurationContainerExtensions
{
    public static TContainer Add<TContainer>(
        this TContainer container,
        object? config
    )
        where TContainer : IConfigurationContainer
    {
        var configuration = new ObjectConfigurationProvider(config).Read();

        container.Add(configuration);

        return container;
    }
}