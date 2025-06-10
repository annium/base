using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.Mapper;
using Annium.Core.Runtime;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Static helper class for creating configured instances of classes
/// </summary>
public static class Configurator
{
    /// <summary>
    /// Creates and configures an instance of type T using the provided configuration
    /// </summary>
    /// <param name="configure">Action to configure the configuration container</param>
    /// <returns>Configured instance of type T</returns>
    public static T Get<T>(Action<IConfigurationContainer> configure)
        where T : class, new()
    {
        var container = GetServices<T>();

        container.AddConfiguration<T>(configure);

        return Get<T>(container);
    }

    /// <summary>
    /// Creates and configures an instance of type T using the provided asynchronous configuration
    /// </summary>
    /// <param name="configure">Async function to configure the configuration container</param>
    /// <returns>Task containing configured instance of type T</returns>
    public static async Task<T> GetAsync<T>(Func<IConfigurationContainer, Task> configure)
        where T : class, new()
    {
        var container = GetServices<T>();

        await container.AddConfigurationAsync<T>(configure);

        return Get<T>(container);
    }

    /// <summary>
    /// Creates and configures a service container with required dependencies for type T
    /// </summary>
    /// <returns>Configured service container</returns>
    private static IServiceContainer GetServices<T>()
    {
        var container = new ServiceContainer();

        container.AddRuntime(typeof(T).Assembly);
        container.AddMapper();

        return container;
    }

    /// <summary>
    /// Resolves an instance of type T from the service container
    /// </summary>
    /// <param name="container">Service container to resolve from</param>
    /// <returns>Instance of type T</returns>
    private static T Get<T>(IServiceContainer container)
        where T : notnull
    {
        var provider = container.BuildServiceProvider();

        var configuration = provider.Resolve<T>();

        return configuration;
    }
}
