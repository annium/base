using System;
using System.Linq;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for retrieving the unbound base type of a <see cref="Type"/>.
/// </summary>
public static class GetUnboundBaseTypeExtension
{
    /// <summary>
    /// Gets the unbound base type of the specified type, resolving any generic type parameters.
    /// </summary>
    /// <param name="type">The type to get the unbound base type from.</param>
    /// <returns>The unbound base type if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static Type? GetUnboundBaseType(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var baseType = type.BaseType;
        if (baseType == null)
            return null;

        if (!baseType.ContainsGenericParameters)
            return baseType;

        var genericArgs = baseType.GetGenericTypeDefinition().GetGenericArguments();
        var unboundBaseArgs = baseType
            .GetGenericArguments()
            .Select((arg, i) => arg.IsGenericParameter ? genericArgs[i] : arg)
            .ToArray();

        return baseType.GetGenericTypeDefinition().TryMakeGenericType(out var result, unboundBaseArgs) ? result : null;
    }
}
