using System;
using System.Linq;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for determining if a <see cref="Type"/> is a curiously recurring generic parameter.
/// </summary>
public static class IsCuriouslyRecurringGenericParameterExtension
{
    /// <summary>
    /// Determines whether the specified type is a curiously recurring generic parameter (CRTP).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is a curiously recurring generic parameter; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    public static bool IsCuriouslyRecurringGenericParameter(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (!type.IsGenericParameter)
            return false;

        return type.GetGenericParameterConstraints().Any(constraint => constraint.GetGenericArguments().Contains(type));
    }
}
