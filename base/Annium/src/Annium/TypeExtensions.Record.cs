using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Annium;

public static class TypeRecordExtensions
{
    public static bool IsRecordLike(this Type type) =>
        type.TryGetRecordElementTypes(out _, out _);

    public static (Type keyType, Type valueType) GetRecordElementTypes(this Type type)
    {
        if (type.TryGetRecordElementTypes(out var keyType, out var valueType))
            return (keyType, valueType);

        throw new InvalidOperationException($"Type {type.FriendlyName()} is not array-like type");
    }

    public static bool TryGetRecordElementTypes(
        this Type type,
        [NotNullWhen(true)] out Type? keyType,
        [NotNullWhen(true)] out Type? valueType
    )
    {
        keyType = null;
        valueType = null;

        var arrayImplementation = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            ? type
            : type.GetInterfaces()
                .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (arrayImplementation is null)
            return false;

        var elementType = arrayImplementation.GetGenericArguments()[0];
        if (!elementType.IsGenericType || elementType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
            return false;

        var elementTypeArguments = elementType.GetGenericArguments();

        keyType = elementTypeArguments[0];
        valueType = elementTypeArguments[1];

        return true;
    }
}