using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Annium.Data.Operations.Serialization.MessagePack.Internal;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Data.Operations.Serialization.MessagePack;

public class Resolver : IFormatterResolver
{
    public static IFormatterResolver Instance { get; } = new Resolver();
    private static readonly ConcurrentDictionary<Type, IMessagePackFormatter> Formatters = new();

    private static readonly IReadOnlyDictionary<Type, IMessagePackFormatter> FormatterInstances = new Dictionary<
        Type,
        IMessagePackFormatter
    >()
    {
        { typeof(IResult), ResultFormatter.Instance },
        { typeof(IBooleanResult), BooleanResultFormatter.Instance },
    };

    private static readonly IReadOnlyDictionary<Type, Type> FormatterInstanceTypes = new Dictionary<Type, Type>()
    {
        { typeof(IResult<>), typeof(ResultDataFormatter<>) },
        { typeof(IBooleanResult<>), typeof(BooleanResultDataFormatter<>) },
        { typeof(IStatusResult<>), typeof(StatusResultFormatter<>) },
        { typeof(IStatusResult<,>), typeof(StatusDataResultFormatter<,>) },
    };

    private Resolver() { }

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        var formatter = (IMessagePackFormatter<T>?)GetFormatterPrivate<T>();
        return formatter;
    }

    private IMessagePackFormatter? GetFormatterPrivate<T>()
    {
        var type = typeof(T);
        if (FormatterInstances.TryGetValue(type, out var formatterInstance))
            return formatterInstance;

        if (
            typeof(T).IsGenericType
            && FormatterInstanceTypes.TryGetValue(type.GetGenericTypeDefinition(), out var formatterBaseType)
        )
            return Formatters.GetOrAdd(type, ResolveInstance, formatterBaseType);

        return null;
    }

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
