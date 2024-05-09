using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Serialization.Abstractions;

public static class ServiceProviderExtensions
{
    public static ISerializer<T> ResolveSerializer<T>(this IServiceProvider provider, string mediaType)
    {
        return provider.ResolveSerializer<T>(Constants.DefaultKey, mediaType);
    }

    public static ISerializer<T> ResolveSerializer<T>(this IServiceProvider provider, string key, string mediaType)
    {
        var serializerKey = SerializerKey.Create(key, mediaType);

        return provider.GetRequiredKeyedService<ISerializer<T>>(serializerKey);
    }

    public static ISerializer<TSource, TDestination> ResolveSerializer<TSource, TDestination>(
        this IServiceProvider provider,
        string mediaType
    )
    {
        return provider.ResolveSerializer<TSource, TDestination>(Constants.DefaultKey, mediaType);
    }

    public static ISerializer<TSource, TDestination> ResolveSerializer<TSource, TDestination>(
        this IServiceProvider provider,
        string key,
        string mediaType
    )
    {
        var serializerKey = SerializerKey.Create(key, mediaType);

        return provider.GetRequiredKeyedService<ISerializer<TSource, TDestination>>(serializerKey);
    }
}
