using System;
using System.Linq;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for retrieving the interfaces directly implemented by a <see cref="Type"/> (excluding inherited interfaces).
/// </summary>
public static class GetOwnInterfacesExtension
{
    /// <summary>
    /// Gets the interfaces directly implemented by the specified type, excluding inherited interfaces.
    /// </summary>
    /// <param name="type">The type to get directly implemented interfaces for.</param>
    /// <returns>An array of <see cref="Type"/> representing the directly implemented interfaces.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the type is not a class, interface, or value type.</exception>
    public static Type[] GetOwnInterfaces(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (type is { IsValueType: false, IsClass: false, IsInterface: false })
            throw new ArgumentException(nameof(type), $"Can't collect inherited interfaces of {type.FriendlyName()}");

        var inheritedInterfaces = type.GetInheritedInterfaces();
        var allInterfaces = type.GetInterfaces();
        var ownInterfaces = allInterfaces.Except(inheritedInterfaces).ToArray();

        return ownInterfaces;
    }
}
