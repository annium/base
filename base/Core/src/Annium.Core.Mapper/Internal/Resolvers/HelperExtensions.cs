using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Resolvers;

internal static class HelperExtensions
{
    public static Type? GetEnumerableElementType(this Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.GenericTypeArguments.Length == 0)
            return null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GenericTypeArguments[0];

        var enumerable = type.GetTypeInfo().ImplementedInterfaces
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerable?.GenericTypeArguments[0];
    }

    public static ConstructorInfo GetParametrizedConstructor(this Type type) => type
            .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.GetParameters().Length > 0)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault()
        ?? throw new InvalidOperationException("Parameterized constructor not found");

    public static ConstructorInfo GetDefaultConstructor(this Type type) => type
            .GetConstructor(Type.EmptyTypes)
        ?? throw new InvalidOperationException("Parameterless constructor not found");

    public static PropertyInfo[] GetReadableProperties(this Type type) => type
        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .Where(x => x.CanRead)
        .ToArray();

    public static PropertyInfo[] GetWriteableProperties(this Type type) => type
        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .Where(x => x.CanWrite)
        .ToArray();
}