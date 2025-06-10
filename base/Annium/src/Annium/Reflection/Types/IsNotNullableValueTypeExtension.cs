using System;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for determining if a <see cref="Type"/> is a non-nullable value type.
/// </summary>
public static class IsNotNullableValueTypeExtension
{
    /// <summary>
    /// Determines whether the specified type is a non-nullable value type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a non-nullable value type; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    public static bool IsNotNullableValueType(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.IsValueType && Nullable.GetUnderlyingType(type) is null;
    }
}
