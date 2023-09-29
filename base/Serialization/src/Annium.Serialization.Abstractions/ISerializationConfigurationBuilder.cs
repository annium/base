using System;

namespace Annium.Serialization.Abstractions;

public interface ISerializationConfigurationBuilder
{
    string Key { get; }

    ISerializationConfigurationBuilder Register<TValue, TSerializer>(string mediaType, bool isDefault)
        where TSerializer : class, ISerializer<TValue>;

    ISerializationConfigurationBuilder Register<TValue, TSerializer>(string mediaType, bool isDefault, Func<IServiceProvider, TSerializer> resolveSerializer)
        where TSerializer : class, ISerializer<TValue>;

    ISerializationConfigurationBuilder Register<TSource, TDestination, TSerializer>(string mediaType, bool isDefault)
        where TSerializer : class, ISerializer<TSource, TDestination>;

    ISerializationConfigurationBuilder Register<TSource, TDestination, TSerializer>(string mediaType, bool isDefault, Func<IServiceProvider, TSerializer> resolveSerializer)
        where TSerializer : class, ISerializer<TSource, TDestination>;
}