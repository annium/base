using System;
using Annium.Core.Mediator;
using Annium.Core.Mediator.Internal;
using Annium.Core.Runtime.Types;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
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

    public static IServiceContainer AddMediatorConfiguration(
        this IServiceContainer container,
        Action<MediatorConfiguration> configure
    )
    {
        var cfg = new MediatorConfiguration();
        configure(cfg);

        return container.AddMediatorConfiguration(cfg);
    }

    public static IServiceContainer AddMediator(this IServiceContainer container)
    {
        container.Add<ChainBuilder>().AsSelf().Singleton();
        container.Add<NextBuilder>().AsSelf().Singleton();
        container.Add<IMediator, Mediator.Internal.Mediator>().AsSelf().Singleton();

        return container;
    }

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