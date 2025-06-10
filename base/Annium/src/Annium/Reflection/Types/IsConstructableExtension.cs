using System;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for determining if a <see cref="Type"/> is constructable (i.e., can be instantiated).
/// </summary>
public static class IsConstructableExtension
{
    /// <summary>
    /// Determines whether the specified type is constructable (i.e., is a non-abstract class or value type).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is constructable; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    public static bool IsConstructable(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return (type.IsClass || type.IsValueType) && !type.IsAbstract;
    }
}
