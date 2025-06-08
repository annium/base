using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Annium;

/// <summary>
/// Provides extension methods for working with array-like types.
/// </summary>
public static class TypeArrayExtensions
{
    /// <summary>
    /// Determines whether the specified type is enumerable.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>true if the type is enumerable; otherwise, false.</returns>
    /// <remarks>Strings are not considered enumerable by this method.</remarks>
    public static bool IsEnumerable(this Type type)
    {
        if (type == typeof(string))
            return false;

        return type.IsArray || type == typeof(IEnumerable) || type.GetInterfaces().Any(x => x == typeof(IEnumerable));
    }

    /// <summary>
    /// Determines whether the specified type is array-like (can be treated as an array).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>true if the type is array-like; otherwise, false.</returns>
    public static bool IsArrayLike(this Type type) => type.TryGetArrayElementType(out _);

    /// <summary>
    /// Gets the element type of an array-like type.
    /// </summary>
    /// <param name="type">The array-like type.</param>
    /// <returns>The element type of the array-like type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the type is not array-like.</exception>
    public static Type GetArrayElementType(this Type type)
    {
        if (type.TryGetArrayElementType(out var elementType))
            return elementType;

        throw new InvalidOperationException($"Type {type.FriendlyName()} is not array-like type");
    }

    /// <summary>
    /// Attempts to get the element type of an array-like type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="elementType">When this method returns, contains the element type if the type is array-like; otherwise, null.</param>
    /// <returns>true if the type is array-like; otherwise, false.</returns>
    /// <remarks>Strings are not considered array-like by this method.</remarks>
    public static bool TryGetArrayElementType(this Type type, [NotNullWhen(true)] out Type? elementType)
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
