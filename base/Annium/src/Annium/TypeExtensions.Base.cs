using System;
using System.Reflection;

namespace Annium;

/// <summary>
/// Provides base extension methods for working with types.
/// </summary>
public static class TypeBaseExtensions
{
    /// <summary>
    /// Gets the default value for the specified type.
    /// </summary>
    /// <param name="type">The type to get the default value for.</param>
    /// <returns>The default value for the type. For value types, this is an instance with all fields set to their default values. For reference types, this is null.</returns>
    public static object? DefaultValue(this Type type)
    {
        return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// Determines whether the specified type is a scalar type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>true if the type is a primitive type, string, decimal, or enum; otherwise, false.</returns>
    public static bool IsScalar(this Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type.IsEnum;
    }
}
