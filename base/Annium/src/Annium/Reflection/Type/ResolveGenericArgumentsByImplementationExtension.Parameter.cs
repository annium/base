using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static partial class ResolveGenericArgumentsByImplementationExtension
{
    private static Type[]? ResolveGenericParameterArgumentsByGenericParameter(this Type type, Type target)
    {
        var typeAttrs = type.GenericParameterAttributes;
        var targetAttrs = target.GenericParameterAttributes;

        // if reference type constraint is not presented
        if (targetAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) &&
            !typeAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if not nullable value type constraint is not presented
        if (targetAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) &&
            !typeAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        // if default parameter constraint is not presented
        if (targetAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) &&
            !typeAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            return null;

        // ensure all parameter constraints are implemented
        return Helper.ParameterMeetsConstraints(type, target) ? new[] { type } : null;
    }

    private static Type[]? ResolveGenericParameterArgumentsByClass(this Type type, Type target)
    {
        // return target, if all parameter constraints are implemented
        return target.CanBeUsedAsParameter(type) ? new[] { target } : null;
    }

    private static Type[]? ResolveGenericParameterArgumentsByStruct(this Type type, Type target)
    {
        // return target, if all parameter constraints are implemented
        return target.CanBeUsedAsParameter(type) ? new[] { target } : null;
    }

    private static Type[]? ResolveGenericParameterArgumentsByInterface(this Type type, Type target)
    {
        // return target, if all parameter constraints are implemented
        return target.CanBeUsedAsParameter(type) ? new[] { target } : null;
    }
}

file class Helper
{
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