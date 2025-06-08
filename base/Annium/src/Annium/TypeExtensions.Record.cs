using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Annium;

/// <summary>
/// Provides extension methods for working with record-like types.
/// </summary>
public static class TypeRecordExtensions
{
    /// <summary>
    /// Determines whether the specified type is record-like (implements IEnumerable of KeyValuePair).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>true if the type is record-like; otherwise, false.</returns>
    public static bool IsRecordLike(this Type type) => type.TryGetRecordElementTypes(out _, out _);

    /// <summary>
    /// Gets the key and value types of a record-like type.
    /// </summary>
    /// <param name="type">The record-like type.</param>
    /// <returns>A tuple containing the key and value types.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the type is not record-like.</exception>
    public static (Type keyType, Type valueType) GetRecordElementTypes(this Type type)
    {
        if (type.TryGetRecordElementTypes(out var keyType, out var valueType))
            return (keyType, valueType);

        throw new InvalidOperationException($"Type {type.FriendlyName()} is not record-like type");
    }

    /// <summary>
    /// Attempts to get the key and value types of a record-like type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="keyType">When this method returns, contains the key type if the type is record-like; otherwise, null.</param>
    /// <param name="valueType">When this method returns, contains the value type if the type is record-like; otherwise, null.</param>
    /// <returns>true if the type is record-like; otherwise, false.</returns>
    public static bool TryGetRecordElementTypes(
        this Type type,
        [NotNullWhen(true)] out Type? keyType,
        [NotNullWhen(true)] out Type? valueType
    )
    {
        keyType = null;
        valueType = null;

        var arrayImplementation =
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
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
