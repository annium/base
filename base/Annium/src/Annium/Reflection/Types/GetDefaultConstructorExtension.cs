using System;
using System.Reflection;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for retrieving the default constructor of a <see cref="Type"/>.
/// </summary>
public static class GetDefaultConstructorExtension
{
    /// <summary>
    /// Gets the default constructor of the specified type using public, non-public, and instance binding flags.
    /// </summary>
    /// <param name="type">The type to get the default constructor for.</param>
    /// <returns>The <see cref="ConstructorInfo"/> representing the default constructor.</returns>
    public static ConstructorInfo GetDefaultConstructor(this Type type) =>
        type.GetDefaultConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>
    /// Tries to get the default constructor of the specified type using public, non-public, and instance binding flags.
    /// </summary>
    /// <param name="type">The type to get the default constructor for.</param>
    /// <returns>The <see cref="ConstructorInfo"/> representing the default constructor, or null if not found.</returns>
    public static ConstructorInfo? TryGetDefaultConstructor(this Type type) =>
        type.TryGetDefaultConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>
    /// Gets the default constructor of the specified type using the provided binding flags.
    /// </summary>
    /// <param name="type">The type to get the default constructor for.</param>
    /// <param name="bindingFlags">The binding flags to use for retrieving the constructor.</param>
    /// <returns>The <see cref="ConstructorInfo"/> representing the default constructor.</returns>
    /// <exception cref="ArgumentException">Thrown if the type has no default constructor.</exception>
    public static ConstructorInfo GetDefaultConstructor(this Type type, BindingFlags bindingFlags) =>
        type.TryGetDefaultConstructor()
        ?? throw new ArgumentException($"{type.FriendlyName()} has no default constructor");

    /// <summary>
    /// Tries to get the default constructor of the specified type using the provided binding flags.
    /// </summary>
    /// <param name="type">The type to get the default constructor for.</param>
    /// <param name="bindingFlags">The binding flags to use for retrieving the constructor.</param>
    /// <returns>The <see cref="ConstructorInfo"/> representing the default constructor, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    public static ConstructorInfo? TryGetDefaultConstructor(this Type type, BindingFlags bindingFlags)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (type.IsClass || type.IsValueType)
            return type.GetConstructor(bindingFlags, Type.EmptyTypes);

        return null;
    }
}
