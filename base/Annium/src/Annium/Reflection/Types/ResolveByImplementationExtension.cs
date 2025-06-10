using System;
using System.Linq;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for resolving a type by its implementation of a target type, possibly with generic parameters.
/// </summary>
public static class ResolveByImplementationExtension
{
    /// <summary>
    /// Gets the implementation of the given type (which may contain generic parameters) that implements the specified concrete target type.
    /// </summary>
    /// <param name="type">The type to resolve, possibly containing generic parameters.</param>
    /// <param name="target">The concrete target type to resolve against.</param>
    /// <returns>The resolved <see cref="Type"/> if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> or <paramref name="target"/> is null.</exception>
    public static Type? ResolveByImplementation(this Type type, Type target)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        // if type is defined - no need for resolution
        if (!type.ContainsGenericParameters)
            return target.IsAssignableFrom(type) ? type : null;

        var args = type.ResolveGenericArgumentsByImplementation(target);
        if (args is null)
            return null;

        if (type.IsGenericParameter)
            return args.SingleOrDefault();

        if (!type.GetGenericTypeDefinition().TryMakeGenericType(out var result, args))
            return null;

        return target.IsAssignableFrom(result) ? result : null;
    }
}
