namespace Annium.Serialization.Abstractions;

public static class SerializerExtensions
{
    public static ISerializer<TResult> Chain<TSource, TResult>(
        this ISerializer<TSource> source,
        ISerializer<TSource, TResult> wrapper
    )
    {
        return Serializer.Create(
            value => wrapper.Serialize(source.Serialize(value)),
            (type, value) => source.Deserialize(type, wrapper.Deserialize(value))
        );
    }

    public static ISerializer<TResult> Chain<TSource, TResult>(
        this ISerializer<TSource> source,
        ISerializer<TResult, TSource> wrapper
    )
    {
        return Serializer.Create(
            value => wrapper.Deserialize(source.Serialize(value)),
            (type, value) => source.Deserialize(type, wrapper.Serialize(value))
        );
    }

    public static ISerializer<TSource, TResult> Chain<TSource, TInterim, TResult>(
        this ISerializer<TSource, TInterim> source,
        ISerializer<TInterim, TResult> wrapper
    )
    {
        return Serializer.Create<TSource, TResult>(
            value => wrapper.Serialize(source.Serialize(value)),
            value => source.Deserialize(wrapper.Deserialize(value))
        );
    }

    public static ISerializer<TSource, TResult> Chain<TSource, TInterim, TResult>(
        this ISerializer<TSource, TInterim> source,
        ISerializer<TResult, TInterim> wrapper
    )
    {
        return Serializer.Create<TSource, TResult>(
            value => wrapper.Deserialize(source.Serialize(value)),
            value => source.Deserialize(wrapper.Serialize(value))
        );
    }
}