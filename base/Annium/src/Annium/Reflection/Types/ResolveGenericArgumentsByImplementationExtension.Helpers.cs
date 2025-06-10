using System;
using System.Linq;
using System.Reflection;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for resolving generic arguments based on type implementations.
/// </summary>
public static partial class ResolveGenericArgumentsByImplementationExtension
{
    /// <summary>
    /// Builds generic arguments for a type based on source and target types.
    /// </summary>
    /// <param name="type">The type to build arguments for.</param>
    /// <param name="source">The source type containing the generic parameters.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>An array of resolved type arguments, or null if resolution fails.</returns>
    private static Type[]? BuildArgs(Type type, Type source, Type target)
    {
        var args = type.GetGenericArguments();

        var succeed = FillArgs(args, source, target);
        if (!succeed)
            return null;

        var unresolvedArgs = args.Count(a => a.IsGenericTypeParameter);
        if (unresolvedArgs == 0 || unresolvedArgs == args.Length)
            return args;

        var originalArgs = type.GetGenericArguments();

        while (true)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.IsGenericTypeParameter)
                    continue;

                foreach (var constraint in originalArgs[i].GetGenericParameterConstraints())
                    if (!FillArgs(args, constraint, arg))
                        return null;
            }

            var currentlyUnresolved = args.Count(a => a.IsGenericTypeParameter);
            if (currentlyUnresolved == 0 || currentlyUnresolved == unresolvedArgs)
                break;

            unresolvedArgs = currentlyUnresolved;
        }

        return args;
    }

    /// <summary>
    /// Fills generic arguments array based on source and target types.
    /// </summary>
    /// <param name="args">The array to fill with resolved arguments.</param>
    /// <param name="source">The source type containing the generic parameters.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <returns>True if arguments were successfully filled; otherwise, false.</returns>
    private static bool FillArgs(Type[] args, Type source, Type target)
    {
        var implementation = target.GetTargetImplementation(source);
        if (implementation is null)
            return false;

        target = implementation;
        Type[] sourceArgs;
        Type[] targetArgs;
        if (target.IsArray)
        {
            sourceArgs = new[] { source.GetElementType()! };
            targetArgs = new[] { target.GetElementType()! };
        }
        else if (target.IsGenericType)
        {
            sourceArgs = source.GetGenericArguments();
            targetArgs = target.GetGenericArguments();
        }
        else
            return false;

        for (var i = 0; i < sourceArgs.Length; i++)
        {
            if (sourceArgs[i].IsGenericParameter)
                args[sourceArgs[i].GenericParameterPosition] = targetArgs[i];
            else if (sourceArgs[i].ContainsGenericParameters)
            {
                if (!FillArgs(args, sourceArgs[i], targetArgs[i]))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines if a type can be used as a generic parameter based on its constraints.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="parameter">The generic parameter to check against.</param>
    /// <returns>True if the type can be used as the parameter; otherwise, false.</returns>
    private static bool CanBeUsedAsParameter(this Type type, Type parameter)
    {
        var parameterAttrs = parameter.GenericParameterAttributes;

        // check reference type constraint
        if (parameterAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && !type.IsClass)
            return false;

        // check not nullable value type constraint
        if (
            parameterAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)
            && !type.IsNotNullableValueType()
        )
            return false;

        // check default parameter constraint
        if (
            parameterAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
            && !(type.IsConstructable() && type.HasDefaultConstructor())
        )
            return false;

        var constraints = parameter.GetGenericParameterConstraints();

        foreach (var constraint in constraints)
        {
            var constraintArgs = type.ResolveGenericArgumentsByImplementation(constraint);
            if (constraintArgs is null)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Attempts to get the target implementation of a type and its generic arguments.
    /// </summary>
    /// <param name="type">The type to check for implementation.</param>
    /// <param name="target">The target type to resolve against.</param>
    /// <param name="args">When this method returns, contains the generic arguments if successful; otherwise, null.</param>
    /// <returns>True if the implementation was found; otherwise, false.</returns>
    private static bool TryGetTargetImplementation(this Type type, Type target, out Type[]? args)
    {
        // if type is not generic - check target implementation and return empty types if implementation is available
        if (!type.IsGenericType)
        {
            args = type.GetTargetImplementation(target) is null ? null : Type.EmptyTypes;
            return true;
        }

        // if type is defined generic - check target implementation and return it's arguments if implementation is available
        if (!type.ContainsGenericParameters)
        {
            args = type.GetTargetImplementation(target) is null ? null : type.GetGenericArguments();
            return true;
        }

        args = null;
        return false;
    }

    /// <summary>
    /// Attempts to check if a type is assignable from another type and get its generic arguments.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="target">The target type to check against.</param>
    /// <param name="args">When this method returns, contains the generic arguments if successful; otherwise, null.</param>
    /// <returns>True if the check was successful; otherwise, false.</returns>
    private static bool TryCheckAssignableFrom(this Type type, Type target, out Type[]? args)
    {
        // is expected to be used only on generic type
        // if target is not generic - return type's generic arguments, if target is implemented
        if (!target.IsGenericType)
        {
            args = target.IsAssignableFrom(type) ? type.GetGenericArguments() : null;
            return true;
        }

        args = null;
        return false;
    }
}
