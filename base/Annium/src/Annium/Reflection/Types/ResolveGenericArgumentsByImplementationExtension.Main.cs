using System;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for resolving generic arguments of a <see cref="Type"/> by its implementation.
/// </summary>
public static partial class ResolveGenericArgumentsByImplementationExtension
{
    /// <summary>
    /// Resolves the generic arguments of the specified type by its implementation of the target type.
    /// </summary>
    /// <param name="type">The type whose generic arguments are to be resolved.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of <see cref="Type"/> representing the resolved generic arguments, or <c>null</c> if resolution fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> or <paramref name="target"/> is null.</exception>
    /// <exception cref="NotImplementedException">Thrown if the resolution logic is not implemented for the given types.</exception>
    public static Type[]? ResolveGenericArgumentsByImplementation(this Type type, Type target)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (type.IsGenericParameter)
            return type.ResolveGenericParameterArgumentsByTarget(target);

        if (type.IsClass)
            return type.ResolveClassArgumentsByTarget(target);

        if (type.IsValueType)
            return type.ResolveStructArgumentsByByTarget(target);

        if (type.IsInterface)
            return type.ResolveInterfaceArgumentsByTarget(target);

        // otherwise - not implemented or don't know how to resolve
        throw NotImplementedException(type, target);
    }

    /// <summary>
    /// Resolves generic arguments for a generic parameter type based on the target type.
    /// </summary>
    /// <param name="type">The generic parameter type to resolve arguments for.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    /// <exception cref="NotImplementedException">Thrown if the resolution logic is not implemented for the given types.</exception>
    private static Type[]? ResolveGenericParameterArgumentsByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveGenericParameterArgumentsByGenericParameter(target);

        if (target.IsClass)
            return type.ResolveGenericParameterArgumentsByClass(target);

        if (target.IsValueType)
            return type.ResolveGenericParameterArgumentsByStruct(target);

        if (target.IsInterface)
            return type.ResolveGenericParameterArgumentsByInterface(target);

        throw NotImplementedException(type, target);
    }

    /// <summary>
    /// Resolves generic arguments for a class type based on the target type.
    /// </summary>
    /// <param name="type">The class type to resolve arguments for.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    /// <exception cref="NotImplementedException">Thrown if the resolution logic is not implemented for the given types.</exception>
    private static Type[]? ResolveClassArgumentsByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveClassArgumentsByGenericParameter(target);

        if (target.IsClass)
            return type.ResolveClassArgumentsByClass(target);

        if (target.IsValueType)
            return null;

        if (target.IsInterface)
            return type.ResolveClassArgumentsByInterface(target);

        throw NotImplementedException(type, target);
    }

    /// <summary>
    /// Resolves generic arguments for a struct type based on the target type.
    /// </summary>
    /// <param name="type">The struct type to resolve arguments for.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    /// <exception cref="NotImplementedException">Thrown if the resolution logic is not implemented for the given types.</exception>
    private static Type[]? ResolveStructArgumentsByByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveStructArgumentsByGenericParameter(target);

        if (target.IsClass)
            return null;

        if (target.IsValueType)
            return type.ResolveStructArgumentsByStruct(target);

        if (target.IsInterface)
            return type.ResolveStructArgumentsByInterface(target);

        throw NotImplementedException(type, target);
    }

    /// <summary>
    /// Resolves generic arguments for an interface type based on the target type.
    /// </summary>
    /// <param name="type">The interface type to resolve arguments for.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    /// <exception cref="NotImplementedException">Thrown if the resolution logic is not implemented for the given types.</exception>
    private static Type[]? ResolveInterfaceArgumentsByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveInterfaceArgumentsByGenericParameter(target);

        if (target.IsClass)
            return null;

        if (target.IsValueType)
            return null;

        if (target.IsInterface)
            return type.ResolveInterfaceArgumentsByInterface(target);

        throw NotImplementedException(type, target);
    }

    /// <summary>
    /// Creates a <see cref="NotImplementedException"/> for cases where resolution logic is not implemented.
    /// </summary>
    /// <param name="type">The type for which resolution was attempted.</param>
    /// <param name="target">The target type for which resolution was attempted.</param>
    /// <returns>A <see cref="NotImplementedException"/> with a descriptive message.</returns>
    private static NotImplementedException NotImplementedException(Type type, Type target) =>
        new($"Can't resolve {type.Name} generic arguments by implementation {target.Name}");
}
