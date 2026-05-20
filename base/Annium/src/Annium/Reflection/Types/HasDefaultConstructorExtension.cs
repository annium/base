using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for checking if a <see cref="Type"/> has a default constructor.
/// </summary>
public static class HasDefaultConstructorExtension
{
    /// <summary>
    /// Checks if the specified type has a default constructor using public, non-public, and instance binding flags.
    /// </summary>
    /// <param name="type">The type to check for a default constructor.</param>
    /// <returns>True if the type has a default constructor; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the type is not constructable.</exception>
    public static bool HasDefaultConstructor(this Type type) =>
        type.HasDefaultConstructor(Constants.AllInstanceBindingFlags);

    /// <summary>
    /// Checks if the specified type has a default constructor using the provided binding flags.
    /// </summary>
    /// <param name="type">The type to check for a default constructor.</param>
    /// <param name="bindingFlags">The binding flags to use for retrieving the constructor.</param>
    /// <returns>True if the type has a default constructor; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the type is not constructable.</exception>
    public static bool HasDefaultConstructor(this Type type, BindingFlags bindingFlags)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        // Open generic type definitions cannot be instantiated via Activator.CreateInstance and so
        // are reported as not constructable. Without this guard, the IsClass branch would silently
        // return false (GetConstructor returns null) and the IsValueType branch would silently return
        // true (no concrete ctors on the open generic) — both inconsistent with the documented
        // ArgumentException contract for non-constructable types.
        if (type.IsGenericTypeDefinition)
            throw new ArgumentException($"{type} is an open generic type definition and is not constructable");

        if (type.IsClass)
            return type.GetConstructor(bindingFlags, Type.EmptyTypes) != null;

        if (type.IsValueType)
            return type.GetConstructors(bindingFlags).Length == 0
                || type.GetConstructor(bindingFlags, Type.EmptyTypes) != null;

        throw new ArgumentException($"{type} is not constructable");
    }
}
