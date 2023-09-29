using System;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class IsConstructableExtension
{
    public static bool IsConstructable(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return (type.IsClass || type.IsValueType) && !type.IsAbstract;
    }
}