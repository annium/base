using System;
using Annium.Core.Mediator;
using Annium.Core.Mediator.Internal;
using Annium.Core.Runtime.Types;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring mediator services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds mediator configuration with access to type manager
    /// </summary>
    /// <param name="container">Service container to configure</param>
    /// <param name="configure">Configuration action with type manager access</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMediatorConfiguration(
        this IServiceContainer container,
        Action<MediatorConfiguration, ITypeManager> configure
    )
    {
        var cfg = new MediatorConfiguration();
        var typeManager = container.GetTypeManager();
        configure(cfg, typeManager);

        return container.AddMediatorConfiguration(cfg);
    }

    /// <summary>
    /// Adds mediator configuration with simple configuration action
    /// </summary>
    /// <param name="container">Service container to configure</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMediatorConfiguration(
        this IServiceContainer container,
        Action<MediatorConfiguration> configure
    )
    {
        var cfg = new MediatorConfiguration();
        configure(cfg);

        return container.AddMediatorConfiguration(cfg);
    }

    /// <summary>
    /// Adds core mediator services to the container
    /// </summary>
    /// <param name="container">Service container to configure</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMediator(this IServiceContainer container)
    {
        container.Add<ChainBuilder>().AsSelf().Singleton();
        container.Add<NextBuilder>().AsSelf().Singleton();
        container.Add<IMediator, Mediator.Internal.Mediator>().AsSelf().Singleton();

        return container;
    }

    /// <summary>
    /// Adds a specific mediator configuration instance to the container
    /// </summary>
    /// <param name="container">Service container to configure</param>
    /// <param name="cfg">Mediator configuration to add</param>
    /// <returns>The service container for method chaining</returns>
    private static IServiceContainer AddMediatorConfiguration(
        this IServiceContainer container,
        MediatorConfiguration cfg
    )
    {
        container.Add(cfg).AsSelf().Singleton();
        foreach (var handler in cfg.Handlers)
            container.Add(handler.Implementation).AsSelf().Scoped();

        return container;
    }
}
