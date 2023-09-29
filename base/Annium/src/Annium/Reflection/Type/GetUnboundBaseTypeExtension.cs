using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class GetUnboundBaseTypeExtension
{
    public static Type? GetUnboundBaseType(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var baseType = type.BaseType;
        if (baseType == null)
            return null;

        if (!baseType.ContainsGenericParameters)
            return baseType;

        var genericArgs = baseType.GetGenericTypeDefinition().GetGenericArguments();
        var unboundBaseArgs = baseType.GetGenericArguments()
            .Select((arg, i) => arg.IsGenericParameter ? genericArgs[i] : arg)
            .ToArray();

        return baseType.GetGenericTypeDefinition().TryMakeGenericType(out var result, unboundBaseArgs) ? result : null;
    }
}