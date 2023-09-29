using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class ResolveByImplementationExtension
{
    // Get implementation of given type, that may contain generic parameters, that implements concrete target type
    public static Type? ResolveByImplementation(this Type type, Type target)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        // if type is defined - no need for resolution
        if (!type.ContainsGenericParameters)
            return target.IsAssignableFrom(type) ? type : null;

        var args = type.ResolveGenericArgumentsByImplementation(target);
        if (args is null)
            return null;

        if (type.IsGenericParameter)
            return args.SingleOrDefault();

        if (!type.GetGenericTypeDefinition().TryMakeGenericType(out var result, args))
            return null;

        return target.IsAssignableFrom(result) ? result : null;
    }
}