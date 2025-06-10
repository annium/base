using System;
using System.Reflection;

namespace Annium.Reflection.Types;

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
        type.HasDefaultConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

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

        if (type.IsClass)
            return type.GetConstructor(bindingFlags, Type.EmptyTypes) != null;

        if (type.IsValueType)
            return type.GetConstructors(bindingFlags).Length == 0
                || type.GetConstructor(bindingFlags, Type.EmptyTypes) != null;

        throw new ArgumentException($"{type} is not constructable");
    }
}
