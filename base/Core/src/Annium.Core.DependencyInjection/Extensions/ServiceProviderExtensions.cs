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

    public static T ResolveKeyed<T>(this IServiceProvider provider, object key)
        where T : notnull
    {
        return provider.GetRequiredKeyedService<T>(key);
    }

    public static T? TryResolveKeyed<T>(this IServiceProvider provider, object key)
        where T : notnull
    {
        return provider.GetKeyedService<T>(key);
    }

    public static object ResolveKeyed(this IServiceProvider provider, Type type, object key)
    {
        return provider.GetRequiredKeyedService(type, key);
    }

    public static object? TryResolveKeyed(this IServiceProvider provider, Type type, object key)
    {
        return provider.CastTo<IKeyedServiceProvider>().GetKeyedService(type, key);
    }
}
