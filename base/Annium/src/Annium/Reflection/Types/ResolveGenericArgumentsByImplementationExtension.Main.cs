using System;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

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

        return type switch
        {
            { IsGenericParameter: true } => type.ResolveGenericParameterArgumentsByTarget(target),
            { IsClass: true } => type.ResolveClassArgumentsByTarget(target),
            { IsValueType: true } => type.ResolveStructArgumentsByTarget(target),
            { IsInterface: true } => type.ResolveInterfaceArgumentsByTarget(target),
            _ => throw NotImplementedException(type, target),
        };
    }

    /// <summary>
    /// Resolves generic arguments for a generic parameter type based on the target type.
    /// </summary>
    /// <param name="type">The generic parameter type to resolve arguments for.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    /// <exception cref="NotImplementedException">Thrown if the resolution logic is not implemented for the given types.</exception>
    private static Type[]? ResolveGenericParameterArgumentsByTarget(this Type type, Type target) =>
        target switch
        {
            { IsGenericParameter: true } => type.ResolveGenericParameterArgumentsByGenericParameter(target),
            { IsClass: true } => type.ResolveGenericParameterArgumentsByClass(target),
            { IsValueType: true } => type.ResolveGenericParameterArgumentsByStruct(target),
            { IsInterface: true } => type.ResolveGenericParameterArgumentsByInterface(target),
            _ => throw NotImplementedException(type, target),
        };

    /// <summary>
    /// Resolves generic arguments for a class type based on the target type.
    /// </summary>
    /// <param name="type">The class type to resolve arguments for.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    /// <exception cref="NotImplementedException">Thrown if the resolution logic is not implemented for the given types.</exception>
    private static Type[]? ResolveClassArgumentsByTarget(this Type type, Type target) =>
        target switch
        {
            { IsGenericParameter: true } => type.ResolveClassArgumentsByGenericParameter(target),
            { IsClass: true } => type.ResolveClassArgumentsByClass(target),
            { IsValueType: true } => null,
            { IsInterface: true } => type.ResolveClassArgumentsByInterface(target),
            _ => throw NotImplementedException(type, target),
        };

    /// <summary>
    /// Resolves generic arguments for a struct type based on the target type.
    /// </summary>
    /// <param name="type">The struct type to resolve arguments for.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    /// <exception cref="NotImplementedException">Thrown if the resolution logic is not implemented for the given types.</exception>
    private static Type[]? ResolveStructArgumentsByTarget(this Type type, Type target) =>
        target switch
        {
            { IsGenericParameter: true } => type.ResolveStructArgumentsByGenericParameter(target),
            { IsClass: true } => null,
            { IsValueType: true } => type.ResolveStructArgumentsByStruct(target),
            { IsInterface: true } => type.ResolveStructArgumentsByInterface(target),
            _ => throw NotImplementedException(type, target),
        };

    /// <summary>
    /// Resolves generic arguments for an interface type based on the target type.
    /// </summary>
    /// <param name="type">The interface type to resolve arguments for.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    /// <exception cref="NotImplementedException">Thrown if the resolution logic is not implemented for the given types.</exception>
    private static Type[]? ResolveInterfaceArgumentsByTarget(this Type type, Type target) =>
        target switch
        {
            { IsGenericParameter: true } => type.ResolveInterfaceArgumentsByGenericParameter(target),
            { IsClass: true } => null,
            { IsValueType: true } => null,
            { IsInterface: true } => type.ResolveInterfaceArgumentsByInterface(target),
            _ => throw NotImplementedException(type, target),
        };

    /// <summary>
    /// Creates a <see cref="NotImplementedException"/> for cases where resolution logic is not implemented.
    /// </summary>
    /// <param name="type">The type for which resolution was attempted.</param>
    /// <param name="target">The target type for which resolution was attempted.</param>
    /// <returns>A <see cref="NotImplementedException"/> with a descriptive message.</returns>
    private static NotImplementedException NotImplementedException(Type type, Type target) =>
        new($"Can't resolve {type.Name} generic arguments by implementation {target.Name}");
}
