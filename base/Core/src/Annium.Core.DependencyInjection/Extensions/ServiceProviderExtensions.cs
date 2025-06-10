using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for IServiceProvider to provide convenient service resolution methods
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Resolves a service of type T from the service provider
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <param name="provider">The service provider</param>
    /// <returns>The resolved service instance</returns>
    public static T Resolve<T>(this IServiceProvider provider)
        where T : notnull
    {
        return provider.GetRequiredService<T>();
    }

    /// <summary>
    /// Attempts to resolve a service of type T from the service provider
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <param name="provider">The service provider</param>
    /// <returns>The resolved service instance, or null if not found</returns>
    public static T? TryResolve<T>(this IServiceProvider provider)
        where T : notnull
    {
        return provider.GetService<T>();
    }

    /// <summary>
    /// Resolves a service of the specified type from the service provider
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="type">The type of service to resolve</param>
    /// <returns>The resolved service instance</returns>
    public static object Resolve(this IServiceProvider provider, Type type)
    {
        return provider.GetRequiredService(type);
    }

    /// <summary>
    /// Attempts to resolve a service of the specified type from the service provider
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="type">The type of service to resolve</param>
    /// <returns>The resolved service instance, or null if not found</returns>
    public static object? TryResolve(this IServiceProvider provider, Type type)
    {
        return provider.GetService(type);
    }

    /// <summary>
    /// Resolves a keyed service of type T from the service provider
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <param name="provider">The service provider</param>
    /// <param name="key">The key associated with the service</param>
    /// <returns>The resolved service instance</returns>
    public static T ResolveKeyed<T>(this IServiceProvider provider, object key)
        where T : notnull
    {
        return provider.GetRequiredKeyedService<T>(key);
    }

    /// <summary>
    /// Attempts to resolve a keyed service of type T from the service provider
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <param name="provider">The service provider</param>
    /// <param name="key">The key associated with the service</param>
    /// <returns>The resolved service instance, or null if not found</returns>
    public static T? TryResolveKeyed<T>(this IServiceProvider provider, object key)
        where T : notnull
    {
        return provider.GetKeyedService<T>(key);
    }

    /// <summary>
    /// Resolves a keyed service of the specified type from the service provider
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="type">The type of service to resolve</param>
    /// <param name="key">The key associated with the service</param>
    /// <returns>The resolved service instance</returns>
    public static object ResolveKeyed(this IServiceProvider provider, Type type, object key)
    {
        return provider.GetRequiredKeyedService(type, key);
    }

    /// <summary>
    /// Attempts to resolve a keyed service of the specified type from the service provider
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="type">The type of service to resolve</param>
    /// <param name="key">The key associated with the service</param>
    /// <returns>The resolved service instance, or null if not found</returns>
    public static object? TryResolveKeyed(this IServiceProvider provider, Type type, object key)
    {
        return provider.CastTo<IKeyedServiceProvider>().GetKeyedService(type, key);
    }
}
