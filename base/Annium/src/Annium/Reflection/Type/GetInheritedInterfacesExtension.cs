using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class GetInheritedInterfacesExtension
{
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