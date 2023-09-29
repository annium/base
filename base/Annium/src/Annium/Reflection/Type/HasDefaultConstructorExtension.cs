using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class HasDefaultConstructorExtension
{
    public static bool HasDefaultConstructor(this Type type) =>
        type.HasDefaultConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    public static bool HasDefaultConstructor(this Type type, BindingFlags bindingFlags)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (type.IsClass)
            return type.GetConstructor(bindingFlags, Type.EmptyTypes) != null;

        if (type.IsValueType)
            return type.GetConstructors(bindingFlags).Length == 0 || type.GetConstructor(bindingFlags, Type.EmptyTypes) != null;

        throw new ArgumentException($"{type} is not constructable");
    }
}