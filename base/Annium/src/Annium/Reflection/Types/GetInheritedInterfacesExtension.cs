using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for retrieving inherited interfaces of a <see cref="Type"/>.
/// </summary>
public static class GetInheritedInterfacesExtension
{
    /// <summary>
    /// Gets all interfaces inherited by the specified type.
    /// </summary>
    /// <param name="type">The type to get inherited interfaces for.</param>
    /// <returns>An array of <see cref="Type"/> representing the inherited interfaces.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the type is not a class, interface, or value type.</exception>
    public static Type[] GetInheritedInterfaces(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (type is { IsValueType: false, IsClass: false, IsInterface: false })
            throw new ArgumentException($"Can't collect inherited interfaces of {type.FriendlyName()}", nameof(type));

        var interfaces = new HashSet<Type>();

        if (type.BaseType is not null)
            foreach (var x in type.BaseType.GetInterfaces())
                interfaces.Add(x);

        foreach (var @interface in type.GetInterfaces())
        foreach (var x in @interface.GetInterfaces())
            interfaces.Add(x);

        return interfaces.ToArray();
    }
}
