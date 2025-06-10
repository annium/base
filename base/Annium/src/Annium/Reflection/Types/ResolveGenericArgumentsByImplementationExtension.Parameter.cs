using System;
using System.Reflection;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for resolving generic arguments for generic parameter types based on target types.
/// </summary>
public static partial class ResolveGenericArgumentsByImplementationExtension
{
    /// <summary>
    /// Resolves generic arguments for a generic parameter type when the target is another generic parameter.
    /// </summary>
    /// <param name="type">The generic parameter type to resolve arguments for.</param>
    /// <param name="target">The target generic parameter type.</param>
    /// <returns>An array containing the resolved type, or null if constraints are not met.</returns>
    private static Type[]? ResolveGenericParameterArgumentsByGenericParameter(this Type type, Type target)
    {
        var typeAttrs = type.GenericParameterAttributes;
        var targetAttrs = target.GenericParameterAttributes;

        // if reference type constraint is not presented
        if (
            targetAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)
            && !typeAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)
        )
            return null;

        // if not nullable value type constraint is not presented
        if (
            targetAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)
            && !typeAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)
        )
            return null;

        // if default parameter constraint is not presented
        if (
            targetAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
            && !typeAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
        )
            return null;

        // ensure all parameter constraints are implemented
        return Helper.ParameterMeetsConstraints(type, target) ? new[] { type } : null;
    }

    /// <summary>
    /// Resolves generic arguments for a generic parameter type when the target is a class type.
    /// </summary>
    /// <param name="type">The generic parameter type to resolve arguments for.</param>
    /// <param name="target">The target class type.</param>
    /// <returns>An array containing the resolved type, or null if constraints are not met.</returns>
    private static Type[]? ResolveGenericParameterArgumentsByClass(this Type type, Type target)
    {
        // return target, if all parameter constraints are implemented
        return target.CanBeUsedAsParameter(type) ? new[] { target } : null;
    }

    /// <summary>
    /// Resolves generic arguments for a generic parameter type when the target is a struct type.
    /// </summary>
    /// <param name="type">The generic parameter type to resolve arguments for.</param>
    /// <param name="target">The target struct type.</param>
    /// <returns>An array containing the resolved type, or null if constraints are not met.</returns>
    private static Type[]? ResolveGenericParameterArgumentsByStruct(this Type type, Type target)
    {
        // return target, if all parameter constraints are implemented
        return target.CanBeUsedAsParameter(type) ? new[] { target } : null;
    }

    /// <summary>
    /// Resolves generic arguments for a generic parameter type when the target is an interface type.
    /// </summary>
    /// <param name="type">The generic parameter type to resolve arguments for.</param>
    /// <param name="target">The target interface type.</param>
    /// <returns>An array containing the resolved type, or null if constraints are not met.</returns>
    private static Type[]? ResolveGenericParameterArgumentsByInterface(this Type type, Type target)
    {
        // return target, if all parameter constraints are implemented
        return target.CanBeUsedAsParameter(type) ? new[] { target } : null;
    }
}

/// <summary>
/// Helper class for checking if a generic parameter meets all constraints of a target parameter.
/// </summary>
file class Helper
{
    /// <summary>
    /// Checks if the source generic parameter meets all constraints of the target generic parameter.
    /// </summary>
    /// <param name="source">The source generic parameter.</param>
    /// <param name="target">The target generic parameter.</param>
    /// <returns>True if all constraints are met; otherwise, false.</returns>
    public static bool ParameterMeetsConstraints(Type source, Type target)
    {
        var sourceConstraints = source.GetGenericParameterConstraints();
        var targetConstraints = target.GetGenericParameterConstraints();
        foreach (var targetConstraint in targetConstraints)
        {
            var meetsConstraint = false;
            foreach (var sourceConstraint in sourceConstraints)
            {
                var constraintArgs = sourceConstraint.ResolveGenericArgumentsByImplementation(targetConstraint);
                if (constraintArgs is null)
                    continue;

                meetsConstraint = true;
                break;
            }

            if (!meetsConstraint)
                return false;
        }

        return true;
    }
}
