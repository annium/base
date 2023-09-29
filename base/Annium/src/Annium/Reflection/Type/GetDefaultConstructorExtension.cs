using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class GetDefaultConstructorExtension
{
    public static ConstructorInfo GetDefaultConstructor(this Type type) =>
        type.GetDefaultConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    public static ConstructorInfo? TryGetDefaultConstructor(this Type type) =>
        type.TryGetDefaultConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    public static ConstructorInfo GetDefaultConstructor(this Type type, BindingFlags bindingFlags) =>
        type.TryGetDefaultConstructor() ?? throw new ArgumentException($"{type.FriendlyName()} has no default constructor");

    public static ConstructorInfo? TryGetDefaultConstructor(this Type type, BindingFlags bindingFlags)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (type.IsClass || type.IsValueType)
            return type.GetConstructor(bindingFlags, Type.EmptyTypes);

        return null;
    }
}