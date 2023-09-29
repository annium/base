using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static partial class ResolveGenericArgumentsByImplementationExtension
{
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

    private static bool CanBeUsedAsParameter(this Type type, Type parameter)
    {
        var parameterAttrs = parameter.GenericParameterAttributes;

        // check reference type constraint
        if (parameterAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && !type.IsClass)
            return false;

        // check not nullable value type constraint
        if (parameterAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) && !type.IsNotNullableValueType())
            return false;

        // check default parameter constraint
        if (parameterAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !(type.IsConstructable() && type.HasDefaultConstructor()))
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