using System;

namespace Annium.Serialization.Abstractions;

public interface ISerializer<TSource, TDestination>
{
    TSource Deserialize(TDestination value);
    TDestination Serialize(TSource value);
}

public interface ISerializer<TValue>
{
    T Deserialize<T>(TValue value);
    object? Deserialize(Type type, TValue value);
    TValue Serialize<T>(T value);
    TValue Serialize(object? value);
}