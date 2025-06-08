using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Resolvers;

/// <summary>
/// Helper extension methods for type inspection and property access
/// </summary>
internal static class HelperExtensions
{
    /// <summary>
    /// Gets the element type of an enumerable type
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>The element type if the type is enumerable, otherwise null</returns>
    public static Type? GetEnumerableElementType(this Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.GenericTypeArguments.Length == 0)
            return null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GenericTypeArguments[0];

        var enumerable = type.GetTypeInfo()
            .ImplementedInterfaces.FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            );

        return enumerable?.GenericTypeArguments[0];
    }

    /// <summary>
    /// Gets the constructor with the most parameters
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>The constructor with the most parameters</returns>
    public static ConstructorInfo GetParametrizedConstructor(this Type type) =>
        type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.GetParameters().Length > 0)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault() ?? throw new InvalidOperationException("Parameterized constructor not found");

    /// <summary>
    /// Gets the default parameterless constructor
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>The default constructor</returns>
    public static ConstructorInfo GetDefaultConstructor(this Type type) =>
        type.GetConstructor(Type.EmptyTypes)
        ?? throw new InvalidOperationException("Parameterless constructor not found");

    /// <summary>
    /// Gets all readable properties of the type
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>Array of readable properties</returns>
    public static PropertyInfo[] GetReadableProperties(this Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.CanRead)
            .ToArray();

    /// <summary>
    /// Gets all writeable properties of the type
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>Array of writeable properties</returns>
    public static PropertyInfo[] GetWriteableProperties(this Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.CanWrite)
            .ToArray();
}
