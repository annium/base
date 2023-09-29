using System;
using Annium.Core.DependencyInjection;

namespace Annium.Serialization.Abstractions.Internal;

internal class SerializationConfigurationBuilder : ISerializationConfigurationBuilder
{
    public string Key { get; }
    private readonly IServiceContainer _container;

    public SerializationConfigurationBuilder(IServiceContainer container, string key)
    {
        _container = container;
        Key = key;
    }

    public ISerializationConfigurationBuilder Register<TValue, TSerializer>(string mediaType, bool isDefault)
        where TSerializer : class, ISerializer<TValue>
        => RegisterInternal<ISerializer<TValue>, TSerializer>(mediaType, isDefault);

    public ISerializationConfigurationBuilder Register<TValue, TSerializer>(string mediaType, bool isDefault, Func<IServiceProvider, TSerializer> resolveSerializer)
        where TSerializer : class, ISerializer<TValue>
        => RegisterInternal<ISerializer<TValue>, TSerializer>(mediaType, isDefault, resolveSerializer);

    public ISerializationConfigurationBuilder Register<TSource, TDestination, TSerializer>(string mediaType, bool isDefault)
        where TSerializer : class, ISerializer<TSource, TDestination>
        => RegisterInternal<ISerializer<TSource, TDestination>, TSerializer>(mediaType, isDefault);

    public ISerializationConfigurationBuilder Register<TSource, TDestination, TSerializer>(string mediaType, bool isDefault, Func<IServiceProvider, TSerializer> resolveSerializer)
        where TSerializer : class, ISerializer<TSource, TDestination>
        => RegisterInternal<ISerializer<TSource, TDestination>, TSerializer>(mediaType, isDefault, resolveSerializer);

    private ISerializationConfigurationBuilder RegisterInternal<TISerializer, TSerializer>(string mediaType, bool isDefault, Func<IServiceProvider, TSerializer> resolveSerializer)
        where TISerializer : class
        where TSerializer : class, TISerializer

    {
        var key = SerializerKey.Create(Key, mediaType);

        // add serializer via resolver
        _container.Add(resolveSerializer).AsKeyed<TISerializer, SerializerKey>(key).Singleton();

        // register defaults
        if (key.IsDefault)
            RegisterDefault<TISerializer>(key, isDefault);

        return this;
    }

    private ISerializationConfigurationBuilder RegisterInternal<TISerializer, TSerializer>(string mediaType, bool isDefault)
        where TISerializer : class
        where TSerializer : TISerializer
    {
        var key = SerializerKey.Create(Key, mediaType);

        // add serializer via type registration
        _container.Add<TSerializer>().AsKeyed<TISerializer, SerializerKey>(key).Singleton();

        // register defaults
        if (key.IsDefault)
            RegisterDefault<TISerializer>(key, isDefault);

        return this;
    }

    private void RegisterDefault<TISerializer>(SerializerKey key, bool isDefault)
        where TISerializer : class
    {
        // for default key - configure as default for media type
        _container.Add<TISerializer>(sp => sp.Resolve<IIndex<SerializerKey, TISerializer>>()[key]).AsKeyed<TISerializer, string>(key.MediaType).Singleton();

        // if default media type - configure as default
        if (isDefault)
            _container.Add<TISerializer>(sp => sp.Resolve<IIndex<SerializerKey, TISerializer>>()[key]).As<TISerializer>().Singleton();
    }
}