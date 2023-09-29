using System;
using Microsoft.Extensions.DependencyInjection;
using AsyncServiceScope = Annium.Core.DependencyInjection.Internal.Container.AsyncServiceScope;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static T Resolve<T>(this IServiceProvider provider)
        where T : notnull
        => provider.GetRequiredService<T>();

    public static object Resolve(this IServiceProvider provider, Type type)
        => provider.GetRequiredService(type);

    public static T ResolveKeyed<TKey, T>(this IServiceProvider provider, TKey key)
        where TKey : notnull
        where T : notnull
        => provider.GetRequiredService<IIndex<TKey, T>>()[key];

    public static IAsyncServiceScope CreateAsyncScope(this IServiceProvider provider)
        => new AsyncServiceScope(provider.CreateScope());
}