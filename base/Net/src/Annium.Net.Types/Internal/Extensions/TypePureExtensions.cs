using System;

namespace Annium.Net.Types.Internal.Extensions;

/// <summary>
/// Provides extension methods for extracting pure type information from generic and non-generic types.
/// </summary>
internal static class TypePureExtensions
{
    /// <summary>
    /// Gets the pure type representation, throwing an exception if it cannot be resolved.
    /// </summary>
    /// <param name="type">The type to get the pure representation for.</param>
    /// <returns>The pure type (generic type definition for generic types, or the type itself for non-generic types).</returns>
    /// <exception cref="InvalidOperationException">Thrown when the pure type cannot be resolved.</exception>
    public static Type GetPure(this Type type)
    {
        return type.TryGetPure() ?? throw new InvalidOperationException($"Can't resolve pure type of {type}");
    }

    /// <summary>
    /// Attempts to get the pure type representation, returning null if it cannot be resolved.
    /// </summary>
    /// <param name="type">The type to get the pure representation for.</param>
    /// <returns>The pure type (generic type definition for generic types, or the type itself for classes, value types, and interfaces), or null if the type cannot be purified.</returns>
    public static Type? TryGetPure(this Type type)
    {
        if (type.IsGenericType)
            return type.GetGenericTypeDefinition();

        return type.IsClass || type.IsValueType || type.IsInterface ? type : null;
    }
}
