using System;

namespace Annium.Serialization.Abstractions.Internal;

internal class SerializerInstance<TSource, TDestination> : ISerializer<TSource, TDestination>
{
    private readonly Func<TSource, TDestination> _serialize;
    private readonly Func<TDestination, TSource> _deserialize;

    public SerializerInstance(
        Func<TSource, TDestination> serialize,
        Func<TDestination, TSource> deserialize
    )
    {
        _deserialize = deserialize;
        _serialize = serialize;
    }

    public TSource Deserialize(TDestination value) => _deserialize(value);

    public TDestination Serialize(TSource value) => _serialize(value);
}

internal class SerializerInstance<TValue> : ISerializer<TValue>
{
    private readonly Func<object?, TValue> _serialize;
    private readonly Func<Type, TValue, object?> _deserialize;

    public SerializerInstance(
        Func<object?, TValue> serialize,
        Func<Type, TValue, object?> deserialize
    )
    {
        _serialize = serialize;
        _deserialize = deserialize;
    }

    public T Deserialize<T>(TValue value) => (T)_deserialize(typeof(T), value)!;

    public object? Deserialize(Type type, TValue value) => _deserialize(type, value);

    public TValue Serialize<T>(T value) => _serialize(value);

    public TValue Serialize(object? value) => _serialize(value);
}