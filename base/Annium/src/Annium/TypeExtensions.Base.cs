using System;
using System.Reflection;

namespace Annium;

public static class TypeBaseExtensions
{
    public static object? DefaultValue(this Type type)
    {
        return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
    }

    public static bool IsScalar(this Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type.IsEnum;
    }
}
