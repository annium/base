using System;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static T Resolve<T>(this IServiceProvider provider)
        where T : notnull
    {
        return provider.GetRequiredService<T>();
    }

    public static T? TryResolve<T>(this IServiceProvider provider)
        where T : notnull
    {
        return provider.GetService<T>();
    }

    public static object Resolve(this IServiceProvider provider, Type type)
    {
        return provider.GetRequiredService(type);
    }

    public static object? TryResolve(this IServiceProvider provider, Type type)
    {
        return provider.GetService(type);
    }

    public static T ResolveKeyed<T>(this IKeyedServiceProvider provider, object key)
        where T : notnull
    {
        return (T)provider.GetRequiredKeyedService(typeof(T), key);
    }

    public static T? TryResolveKeyed<T>(this IKeyedServiceProvider provider, object key)
        where T : notnull
    {
        var service = provider.GetKeyedService(typeof(T), key);

        return service is not null ? (T)service : default;
    }

    public static object ResolveKeyed(this IKeyedServiceProvider provider, Type type, object key)
    {
        return provider.GetRequiredKeyedService(type, key);
    }

    public static object? TryResolveKeyed(this IKeyedServiceProvider provider, Type type, object key)
    {
        return provider.GetKeyedService(type, key);
    }
}
