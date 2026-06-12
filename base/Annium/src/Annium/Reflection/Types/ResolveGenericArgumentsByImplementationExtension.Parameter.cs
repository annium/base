using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

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
        return ParameterHelper.ParameterMeetsConstraints(type, target) ? [type] : null;
    }

    /// <summary>
    /// Resolves generic arguments for a generic parameter type when the target is a concrete type
    /// (class, struct, or interface). The resolution is identical for all three target kinds.
    /// </summary>
    /// <param name="type">The generic parameter type to resolve arguments for.</param>
    /// <param name="target">The target type (class, struct, or interface).</param>
    /// <returns>An array containing the resolved type, or null if constraints are not met.</returns>
    private static Type[]? ResolveGenericParameterArgumentsByConcrete(this Type type, Type target)
    {
        // return target, if all parameter constraints are implemented
        return target.CanBeUsedAsParameter(type) ? [target] : null;
    }
}

/// <summary>
/// Helper class for checking if a generic parameter meets all constraints of a target parameter.
/// </summary>
file class ParameterHelper
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
