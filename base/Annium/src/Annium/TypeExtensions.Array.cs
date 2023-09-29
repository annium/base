using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Annium;

public static class TypeArrayExtensions
{
    public static bool IsEnumerable(this Type type)
    {
        if (type == typeof(string))
            return false;

        return type.IsArray || type == typeof(IEnumerable) || type.GetInterfaces().Any(x => x == typeof(IEnumerable));
    }

    public static bool IsArrayLike(this Type type) => type.TryGetArrayElementType(out _);

    public static Type GetArrayElementType(this Type type)
    {
        if (type.TryGetArrayElementType(out var elementType))
            return elementType;

        throw new InvalidOperationException($"Type {type.FriendlyName()} is not array-like type");
    }

    public static bool TryGetArrayElementType(
        this Type type,
        [NotNullWhen(true)] out Type? elementType
    )
    {
        elementType = null;
        if (type == typeof(string))
            return false;

        if (type.IsArray)
        {
            elementType = type.GetElementType();

            return elementType is not null;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            elementType = type.GetGenericArguments()[0];

            return true;
        }

        var arrayImplementation = type.GetInterfaces()
            .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        elementType = arrayImplementation?.GetGenericArguments()[0];

        return elementType is not null;
    }
}