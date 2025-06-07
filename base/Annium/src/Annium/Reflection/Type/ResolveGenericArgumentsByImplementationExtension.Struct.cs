using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for resolving generic arguments for struct types based on target types.
/// </summary>
public static partial class ResolveGenericArgumentsByImplementationExtension
{
    /// <summary>
    /// Resolves generic arguments for a struct type when the target is a generic parameter.
    /// </summary>
    /// <param name="type">The struct type to resolve arguments for.</param>
    /// <param name="target">The target generic parameter type.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    private static Type[]? ResolveStructArgumentsByGenericParameter(this Type type, Type target)
    {
        if (type.TryGetTargetImplementation(target, out var args))
            return args;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

        return type.CanBeUsedAsParameter(target) ? type.GetGenericArguments() : null;
    }

    /// <summary>
    /// Resolves generic arguments for a struct type when the target is another struct type.
    /// </summary>
    /// <param name="type">The struct type to resolve arguments for.</param>
    /// <param name="target">The target struct type.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    private static Type[]? ResolveStructArgumentsByStruct(this Type type, Type target)
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

        return null;
    }

    /// <summary>
    /// Resolves generic arguments for a struct type when the target is an interface type.
    /// </summary>
    /// <param name="type">The struct type to resolve arguments for.</param>
    /// <param name="target">The target interface type.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    private static Type[]? ResolveStructArgumentsByInterface(this Type type, Type target)
    {
        if (type.TryGetTargetImplementation(target, out var args))
            return args;

        if (type.TryCheckAssignableFrom(target, out args))
            return args;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

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
