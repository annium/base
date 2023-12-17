using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IFactoryRegistrationBuilderBase Add<T>(
        this IServiceContainer container,
        Func<IServiceProvider, T> factory
    )
        where T : class
    {
        return container.Add(typeof(T), factory);
    }

    public static IKeyedFactoryRegistrationBuilderBase Add<T>(
        this IServiceContainer container,
        Func<IServiceProvider, object, T> factory
    )
        where T : class
    {
        return container.Add(typeof(T), factory);
    }

    public static ISingleRegistrationBuilderBase Add<TService, TImplementation>(this IServiceContainer container)
        where TImplementation : TService
    {
        return container.Add(typeof(TImplementation)).As<TService>();
    }

    public static ISingleRegistrationBuilderBase Add<TImplementation>(this IServiceContainer container)
    {
        return container.Add(typeof(TImplementation));
    }
}
