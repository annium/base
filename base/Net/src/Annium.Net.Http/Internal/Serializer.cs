using System;
using System.Collections.Concurrent;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Net.Http.Internal;

internal class Serializer
{
    private readonly ConcurrentDictionary<string, ISerializer<string>> _cache = new();
    private readonly IServiceProvider _sp;
    private readonly string _key;

    public Serializer(IServiceProvider sp, string key)
    {
        _sp = sp;
        _key = key;
    }

    public string Serialize<T>(string mediaType, T value)
    {
        var serializer = GetSerializer(mediaType);

        return serializer.Serialize(value);
    }

    public T Deserialize<T>(string mediaType, string value)
    {
        var serializer = GetSerializer(mediaType);

        return serializer.Deserialize<T>(value);
    }

    private ISerializer<string> GetSerializer(string mediaType)
    {
        return _cache.GetOrAdd(mediaType, ResolveSerializer);
    }

    private ISerializer<string> ResolveSerializer(string mediaType)
    {
        var serializerKey = SerializerKey.Create(_key, mediaType);
        var serializer = _sp.ResolveKeyed<ISerializer<string>>(serializerKey);

        if (serializer is null)
            throw new NotSupportedException($"Media type '{mediaType}' is not supported");

        return serializer;
    }
}
