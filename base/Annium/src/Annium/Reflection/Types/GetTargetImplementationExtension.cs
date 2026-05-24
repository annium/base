using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for resolving the implementation of a target type (possibly generic) by a given type.
/// </summary>
public static class GetTargetImplementationExtension
{
    /// <summary>
    /// Gets the implementation of the specified target type by the given type.
    /// </summary>
    /// <param name="type">The concrete type to check for implementation.</param>
    /// <param name="target">The target type, possibly containing generic parameters.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> or <paramref name="target"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> contains generic parameters.</exception>
    public static Type? GetTargetImplementation(this Type type, Type target) =>
        type.GetTargetImplementation(target, new HashSet<Type>());

    /// <summary>
    /// Gets the implementation of the specified target type by the given type, using a set of known generic parameters to avoid cyclic recursion.
    /// </summary>
    /// <param name="type">The concrete type to check for implementation.</param>
    /// <param name="target">The target type, possibly containing generic parameters.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetTargetImplementation(this Type type, Type target, HashSet<Type> genericParameters)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        // can't resolve implementation by type with generic parameters
        if (type.ContainsGenericParameters)
            throw new ArgumentException("Can't resolve implementation of generic type with parameters");

        // if can assign type to target - just return target
        if (target.IsAssignableFrom(type))
            return target;

        // target is not assignable from type and doesn't contain generic parameters - no way for resolution
        if (!target.ContainsGenericParameters)
            return null;

        // as of here:
        // - type is concrete type (no generic parameters)
        // - target is open type with generic parameters

        var implementation = type switch
        {
            { IsClass: true } => type.GetClassImplementationOfTarget(target, genericParameters),
            { IsValueType: true } => type.GetStructImplementationOfTarget(target, genericParameters),
            { IsInterface: true } => type.GetInterfaceImplementationOfTarget(target, genericParameters),
            _ => throw GetException(type, target),
        };

        if (implementation is null)
            return null;

        return implementation.IsAssignableFrom(type) ? implementation : null;
    }

    /// <summary>
    /// Gets the implementation of the target type by a class type.
    /// </summary>
    /// <param name="type">The class type to check for implementation.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetClassImplementationOfTarget(this Type type, Type target, HashSet<Type> genericParameters) =>
        target switch
        {
            { IsGenericParameter: true } => type.GetClassImplementationOfGenericParameter(target, genericParameters),
            { IsClass: true } => type.GetClassImplementationOfClass(target, genericParameters),
            { IsValueType: true } => null,
            { IsInterface: true } => type.GetClassImplementationOfInterface(target, genericParameters),
            _ => throw GetException(type, target),
        };

    /// <summary>
    /// Gets the implementation of the target type by a struct type.
    /// </summary>
    /// <param name="type">The struct type to check for implementation.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetStructImplementationOfTarget(
        this Type type,
        Type target,
        HashSet<Type> genericParameters
    ) =>
        target switch
        {
            { IsGenericParameter: true } => type.GetStructImplementationOfGenericParameter(target, genericParameters),
            { IsClass: true } => null,
            { IsValueType: true } => type.GetStructImplementationOfStruct(target, genericParameters),
            { IsInterface: true } => type.GetStructImplementationOfInterface(target, genericParameters),
            _ => throw GetException(type, target),
        };

    /// <summary>
    /// Gets the implementation of the target type by an interface type.
    /// </summary>
    /// <param name="type">The interface type to check for implementation.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetInterfaceImplementationOfTarget(
        this Type type,
        Type target,
        HashSet<Type> genericParameters
    ) =>
        target switch
        {
            { IsGenericParameter: true } => type.GetInterfaceImplementationOfGenericParameter(
                target,
                genericParameters
            ),
            { IsClass: true } => null,
            { IsValueType: true } => null,
            { IsInterface: true } => type.GetInterfaceImplementationOfInterface(target, genericParameters),
            _ => throw GetException(type, target),
        };

    /// <summary>
    /// Gets the implementation of a generic parameter by a class type.
    /// </summary>
    /// <param name="type">The class type to check for implementation.</param>
    /// <param name="target">The generic parameter to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetClassImplementationOfGenericParameter(
        this Type type,
        Type target,
        HashSet<Type> genericParameters
    )
    {
        genericParameters.Add(target);

        var attrs = target.GenericParameterAttributes;

        // if not nullable value type required - return null
        if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        // if default constructor required, but not present
        if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !type.HasDefaultConstructor())
            return null;

        var meetsConstraints = target
            .GetGenericParameterConstraints()
            .All(constraint => type.GetTargetImplementation(constraint, genericParameters) != null);

        return meetsConstraints ? type : null;
    }

    /// <summary>
    /// Gets the implementation of a class type by another class type.
    /// </summary>
    /// <param name="type">The class type to check for implementation.</param>
    /// <param name="target">The target class type to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetClassImplementationOfClass(this Type type, Type target, HashSet<Type> genericParameters)
    {
        // special handling for array types (array is not generic type, but can contain generic parameters)
        if (target.IsArray)
        {
            if (!type.IsArray)
                return null;

            // Both types are arrays here (target.IsArray checked above, type.IsArray enforced by the early return),
            // so GetElementType() is non-null.
            var elementImplementation = type.GetElementType()!
                .GetTargetImplementation(target.GetElementType()!, genericParameters);

            return elementImplementation?.MakeArrayType();
        }

        var targetBase = target.GetGenericTypeDefinition();
        var implementation = type;

        // go deep in inheritance, until targetBase implementation found
        while (implementation != null)
        {
            if (implementation.IsGenericType && implementation.GetGenericTypeDefinition() == targetBase)
                break;

            implementation = implementation.BaseType;
        }

        if (implementation is null)
            return null;

        return BuildImplementation(implementation, target, genericParameters);
    }

    /// <summary>
    /// Gets the implementation of an interface by a class type.
    /// </summary>
    /// <param name="type">The class type to check for implementation.</param>
    /// <param name="target">The target interface to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetClassImplementationOfInterface(
        this Type type,
        Type target,
        HashSet<Type> genericParameters
    ) => FindInterfaceImplementation(type, target, genericParameters);

    /// <summary>
    /// Gets the implementation of a generic parameter by a struct type.
    /// </summary>
    /// <param name="type">The struct type to check for implementation.</param>
    /// <param name="target">The generic parameter to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetStructImplementationOfGenericParameter(
        this Type type,
        Type target,
        HashSet<Type> genericParameters
    )
    {
        genericParameters.Add(target);

        var attrs = target.GenericParameterAttributes;

        // if reference type required, but target is not class
        if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if default constructor required, but not present
        if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !type.HasDefaultConstructor())
            return null;

        // if not nullable value type required, but target is nullable value type
        if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) && type.IsNullableValueType())
            return null;

        var meetsConstraints = target
            .GetGenericParameterConstraints()
            .All(constraint => type.GetTargetImplementation(constraint, genericParameters) != null);

        return meetsConstraints ? type : null;
    }

    /// <summary>
    /// Gets the implementation of a struct type by another struct type.
    /// </summary>
    /// <param name="type">The struct type to check for implementation.</param>
    /// <param name="target">The target struct type to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetStructImplementationOfStruct(this Type type, Type target, HashSet<Type> genericParameters)
    {
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != target.GetGenericTypeDefinition())
            return null;

        return BuildImplementation(type, target, genericParameters);
    }

    /// <summary>
    /// Gets the implementation of an interface by a struct type.
    /// </summary>
    /// <param name="type">The struct type to check for implementation.</param>
    /// <param name="target">The target interface to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetStructImplementationOfInterface(
        this Type type,
        Type target,
        HashSet<Type> genericParameters
    ) => FindInterfaceImplementation(type, target, genericParameters);

    /// <summary>
    /// Finds the interface among <paramref name="type"/>'s implemented interfaces whose generic
    /// type definition matches that of <paramref name="target"/>, then builds the concrete
    /// implementation. Shared body for class-of-interface and struct-of-interface lookups.
    /// </summary>
    /// <param name="type">The type (class or struct) to scan for implementations.</param>
    /// <param name="target">The target interface to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? FindInterfaceImplementation(Type type, Type target, HashSet<Type> genericParameters)
    {
        var targetBase = target.GetGenericTypeDefinition();
        var implementation = Array.Find(
            type.GetInterfaces(),
            i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase
        );

        if (implementation is null)
            return null;

        return BuildImplementation(implementation, target, genericParameters);
    }

    /// <summary>
    /// Gets the implementation of a generic parameter by an interface type.
    /// </summary>
    /// <param name="type">The interface type to check for implementation.</param>
    /// <param name="target">The generic parameter to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetInterfaceImplementationOfGenericParameter(
        this Type type,
        Type target,
        HashSet<Type> genericParameters
    )
    {
        genericParameters.Add(target);

        var attrs = target.GenericParameterAttributes;

        // if reference type required
        if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if not nullable value type required
        if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        var meetsConstraints = target
            .GetGenericParameterConstraints()
            .All(constraint => type.GetTargetImplementation(constraint, genericParameters) != null);

        return meetsConstraints ? type : null;
    }

    /// <summary>
    /// Gets the implementation of an interface by another interface type.
    /// </summary>
    /// <param name="type">The interface type to check for implementation.</param>
    /// <param name="target">The target interface to resolve against.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the implementation, or <c>null</c> if not found.</returns>
    private static Type? GetInterfaceImplementationOfInterface(
        this Type type,
        Type target,
        HashSet<Type> genericParameters
    )
    {
        var targetBase = target.GetGenericTypeDefinition();
        Type[] candidates = [.. type.GetInterfaces(), type];
        var implementation = Array.Find(candidates, i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

        if (implementation is null)
            return null;

        return BuildImplementation(implementation, target, genericParameters);
    }

    /// <summary>
    /// Builds the implementation of a target type by a given implementation type.
    /// </summary>
    /// <param name="implementation">The generic type, base for the type whose generic definition is constructed against the target.</param>
    /// <param name="target">The implementation target.</param>
    /// <param name="genericParameters">A set of known generic parameters to avoid cyclic recursion.</param>
    /// <returns>The <see cref="Type"/> representing the built implementation, or <c>null</c> if not found.</returns>
    private static Type? BuildImplementation(Type implementation, Type target, HashSet<Type> genericParameters)
    {
        if (target.IsGenericTypeDefinition)
            return implementation;

        var implementationArgs = implementation.GetGenericArguments();
        var targetArgs = target.GenericTypeArguments;
        var args = new Type?[targetArgs.Length];
        for (var i = 0; i < targetArgs.Length; i++)
        {
            var targetArg = targetArgs[i];
            var implementationArg = implementationArgs[i];
            // special case to avoid recursion
            if (genericParameters.Contains(targetArg))
                args[i] = implementationArg;
            // if targetArg is generic parameter, or contains those - go resolve deeper
            else
                args[i] = targetArg.ContainsGenericParameters
                    ? implementationArg.GetTargetImplementation(targetArg)
                    : targetArg;
        }
        foreach (var arg in args)
        {
            if (arg is null)
                return null;
        }

        // The loop above returns null on any null arg, so args has no null elements here.
        if (!target.GetGenericTypeDefinition().TryMakeGenericType(out var result, args!))
            return null;

        return result;
    }

    /// <summary>
    /// Builds a <see cref="NotImplementedException"/> for the case when the implementation cannot be resolved.
    /// </summary>
    /// <param name="type">The type for which the implementation was attempted.</param>
    /// <param name="target">The target type for which the implementation was attempted.</param>
    /// <returns>A <see cref="NotImplementedException"/> with a descriptive message.</returns>
    private static NotImplementedException GetException(Type type, Type target) =>
        new($"Can't resolve {type.Name} implementation of {target.Name}");
}
