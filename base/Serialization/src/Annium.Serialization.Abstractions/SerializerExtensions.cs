namespace Annium.Serialization.Abstractions;

/// <summary>
/// Extension methods for chaining serializers together.
/// </summary>
public static class SerializerExtensions
{
    /// <summary>
    /// Chains two serializers together, using the wrapper to convert the output of the source serializer.
    /// </summary>
    /// <typeparam name="TSource">The intermediate serialization format.</typeparam>
    /// <typeparam name="TResult">The final serialization format.</typeparam>
    /// <param name="source">The source serializer.</param>
    /// <param name="wrapper">The wrapper serializer that converts from the source format to the result format.</param>
    /// <returns>A chained serializer that produces the result format.</returns>
    public static ISerializer<TResult> Chain<TSource, TResult>(
        this ISerializer<TSource> source,
        ISerializer<TSource, TResult> wrapper
    )
    {
        return Serializer.Create(
            (type, value) => wrapper.Serialize(source.Serialize(type, value)),
            (type, value) => source.Deserialize(type, wrapper.Deserialize(value))
        );
    }

    /// <summary>
    /// Chains two serializers together, using the wrapper to convert to the output format from the source serializer's input.
    /// </summary>
    /// <typeparam name="TSource">The intermediate serialization format.</typeparam>
    /// <typeparam name="TResult">The final serialization format.</typeparam>
    /// <param name="source">The source serializer.</param>
    /// <param name="wrapper">The wrapper serializer that converts from the result format to the source format.</param>
    /// <returns>A chained serializer that produces the result format.</returns>
    public static ISerializer<TResult> Chain<TSource, TResult>(
        this ISerializer<TSource> source,
        ISerializer<TResult, TSource> wrapper
    )
    {
        return Serializer.Create(
            (type, value) => wrapper.Deserialize(source.Serialize(type, value)),
            (type, value) => source.Deserialize(type, wrapper.Serialize(value))
        );
    }

    /// <summary>
    /// Chains two type-specific serializers together.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TInterim">The intermediate type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="source">The source serializer.</param>
    /// <param name="wrapper">The wrapper serializer.</param>
    /// <returns>A chained serializer from source to result type.</returns>
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

    /// <summary>
    /// Chains two type-specific serializers together with reverse wrapper direction.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TInterim">The intermediate type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="source">The source serializer.</param>
    /// <param name="wrapper">The wrapper serializer that converts from result to interim type.</param>
    /// <returns>A chained serializer from source to result type.</returns>
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
