using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for resolving generic arguments for interface types based on target types.
/// </summary>
public static partial class ResolveGenericArgumentsByImplementationExtension
{
    /// <summary>
    /// Resolves generic arguments for an interface type when the target is a generic parameter.
    /// </summary>
    /// <param name="type">The interface type to resolve arguments for.</param>
    /// <param name="target">The target generic parameter type.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    private static Type[]? ResolveInterfaceArgumentsByGenericParameter(this Type type, Type target)
    {
        if (type.TryGetTargetImplementation(target, out var args))
            return args;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

        return type.CanBeUsedAsParameter(target) ? type.GetGenericArguments() : null;
    }

    /// <summary>
    /// Resolves generic arguments for an interface type when the target is another interface type.
    /// </summary>
    /// <param name="type">The interface type to resolve arguments for.</param>
    /// <param name="target">The target interface type.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    private static Type[]? ResolveInterfaceArgumentsByInterface(this Type type, Type target)
    {
        if (type.TryGetTargetImplementation(target, out var args))
            return args;

        if (type.TryCheckAssignableFrom(target, out args))
            return args;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

        if (type.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
            return BuildArgs(type, type, target);

        // find interface, that is implementation of target's generic definition
        var targetBase = target.GetGenericTypeDefinition();
        var implementation = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

        if (implementation is null)
            return null;

        // implementation is generic interface type with same base definition, as target
        return BuildArgs(type, implementation, target);
    }
}
