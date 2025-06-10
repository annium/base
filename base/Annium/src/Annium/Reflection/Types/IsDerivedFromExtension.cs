using System;
using System.Linq;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for determining if a <see cref="Type"/> is derived from another type, including generic type definitions.
/// </summary>
public static class IsDerivedFromExtension
{
    /// <summary>
    /// Determines whether the specified type is derived from the target type, including support for generic type definitions.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="target">The target type to check against.</param>
    /// <param name="self">If true, includes the type itself in the check.</param>
    /// <returns><c>true</c> if the type is derived from the target; otherwise, <c>false</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the target type cannot be derived from.</exception>
    public static bool IsDerivedFrom(this Type type, Type target, bool self = false)
    {
        // if target is not generic type definition - simply check with IsAssignable from
        if (!target.IsGenericTypeDefinition)
        {
            if (!self && type == target)
                return false;

            return target.IsAssignableFrom(type);
        }

        if (target.IsClass)
            return GetInheritanceChainExtension
                .GetInheritanceChain(type, self, true)
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);

        if (target.IsInterface)
        {
            if (self && type.IsGenericType && type.GetGenericTypeDefinition() == target)
                return true;

            return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);
        }

        throw new InvalidOperationException($"Type '{target}' cannot be derived from");
    }
}
