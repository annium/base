using System;
using System.Linq;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for resolving generic arguments by implementation for class types.
/// </summary>
public static partial class ResolveGenericArgumentsByImplementationExtension
{
    /// <summary>
    /// Resolves generic arguments for a class type when the target is a generic parameter.
    /// </summary>
    /// <param name="type">The class type to resolve arguments for.</param>
    /// <param name="target">The target generic parameter type.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    private static Type[]? ResolveClassArgumentsByGenericParameter(this Type type, Type target)
    {
        if (type.TryGetTargetImplementation(target, out var args))
            return args;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

        return type.CanBeUsedAsParameter(target) ? type.GetGenericArguments() : null;
    }

    /// <summary>
    /// Resolves generic arguments for a class type when the target is another class type.
    /// </summary>
    /// <param name="type">The class type to resolve arguments for.</param>
    /// <param name="target">The target class type.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    private static Type[]? ResolveClassArgumentsByClass(this Type type, Type target)
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

        var baseType = type.BaseType;

        // if no base type or it's not generic - resolution fails, cause types' generic definitions are different
        if (baseType is null || !baseType.IsGenericType)
            return null;

        // base type is generic class type with same base definition, as target
        if (baseType.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
            return BuildArgs(type, baseType, target);

        // try resolve base type
        return Helper.ResolveBase(type, target);
    }

    /// <summary>
    /// Resolves generic arguments for a class type when the target is an interface type.
    /// </summary>
    /// <param name="type">The class type to resolve arguments for.</param>
    /// <param name="target">The target interface type.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    private static Type[]? ResolveClassArgumentsByInterface(this Type type, Type target)
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

        // implementation is generic interface type with same base definition, as target
        if (implementation != null)
            return BuildArgs(type, implementation, target);

        if (type.BaseType is null)
            return null;

        // try resolve base type
        return Helper.ResolveBase(type, target);
    }
}

/// <summary>
/// Helper class for resolving generic arguments by implementation.
/// </summary>
file class Helper
{
    /// <summary>
    /// Resolves generic arguments for a base type.
    /// </summary>
    /// <param name="type">The type to resolve arguments for.</param>
    /// <param name="target">The target type.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    public static Type[]? ResolveBase(Type type, Type target)
    {
        var unboundBaseType = type.GetUnboundBaseType();
        var baseArgs = unboundBaseType!.ResolveGenericArgumentsByImplementation(target);
        if (baseArgs is null)
            return null;

        if (!type.BaseType!.GetGenericTypeDefinition().TryMakeGenericType(out var baseImplementation, baseArgs))
            return null;

        return type.ResolveGenericArgumentsByImplementation(baseImplementation!);
    }
}
