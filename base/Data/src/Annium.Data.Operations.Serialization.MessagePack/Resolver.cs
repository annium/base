using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Annium.Data.Operations.Serialization.MessagePack.Internal;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Data.Operations.Serialization.MessagePack;

/// <summary>
/// MessagePack formatter resolver for Data.Operations types
/// </summary>
public class Resolver : IFormatterResolver
{
    /// <summary>
    /// Gets the singleton instance of the resolver
    /// </summary>
    public static IFormatterResolver Instance { get; } = new Resolver();

    /// <summary>
    /// Cache for dynamically resolved formatters
    /// </summary>
    private static readonly ConcurrentDictionary<Type, IMessagePackFormatter> _formatters = new();

    /// <summary>
    /// Static formatter instances for non-generic types
    /// </summary>
    private static readonly IReadOnlyDictionary<Type, IMessagePackFormatter> _formatterInstances = new Dictionary<
        Type,
        IMessagePackFormatter
    >
    {
        { typeof(IResult), ResultFormatter.Instance },
        { typeof(IBooleanResult), BooleanResultFormatter.Instance },
    };

    /// <summary>
    /// Mapping of generic type definitions to their corresponding formatter types
    /// </summary>
    private static readonly IReadOnlyDictionary<Type, Type> _formatterInstanceTypes = new Dictionary<Type, Type>
    {
        { typeof(IResult<>), typeof(ResultDataFormatter<>) },
        { typeof(IBooleanResult<>), typeof(BooleanResultDataFormatter<>) },
        { typeof(IStatusResult<>), typeof(StatusResultFormatter<>) },
        { typeof(IStatusResult<,>), typeof(StatusDataResultFormatter<,>) },
    };

    private Resolver() { }

    /// <summary>
    /// Gets the formatter for the specified type
    /// </summary>
    /// <typeparam name="T">The type to get a formatter for</typeparam>
    /// <returns>The formatter for the type, or null if not supported</returns>
    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        var formatter = (IMessagePackFormatter<T>?)GetFormatterPrivate<T>();
        return formatter;
    }

    /// <summary>
    /// Internal method to get formatter for the specified type
    /// </summary>
    /// <typeparam name="T">The type to get a formatter for</typeparam>
    /// <returns>The formatter for the type, or null if not supported</returns>
    private IMessagePackFormatter? GetFormatterPrivate<T>()
    {
        var type = typeof(T);
        if (_formatterInstances.TryGetValue(type, out var formatterInstance))
            return formatterInstance;

        if (
            typeof(T).IsGenericType
            && _formatterInstanceTypes.TryGetValue(type.GetGenericTypeDefinition(), out var formatterBaseType)
        )
            return _formatters.GetOrAdd(type, ResolveInstance, formatterBaseType);

        return null;
    }

    /// <summary>
    /// Resolves a formatter instance for a generic type using reflection
    /// </summary>
    /// <param name="type">The concrete generic type to create a formatter for</param>
    /// <param name="formatterBaseType">The generic formatter type definition</param>
    /// <returns>The resolved formatter instance</returns>
    private static IMessagePackFormatter ResolveInstance(Type type, Type formatterBaseType)
    {
        // extract type args
        var typeArgs = type.GetGenericArguments();

        // build formatter type with type args
        var formatterType = formatterBaseType.MakeGenericType(typeArgs);

        // resolve instance property
        var instanceProperty = formatterType
            .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
            .NotNull();

        // and finally - instance itself
        var instance = instanceProperty.GetValue(null).NotNull();

        return (IMessagePackFormatter)instance;
    }
}
