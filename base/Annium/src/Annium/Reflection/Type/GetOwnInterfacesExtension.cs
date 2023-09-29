using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class GetOwnInterfacesExtension
{
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