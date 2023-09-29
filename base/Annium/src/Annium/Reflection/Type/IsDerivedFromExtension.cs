using System;
using System.Linq;

// ReSharper disable CheckNamespace
namespace Annium.Reflection;

public static class IsDerivedFromExtension
{
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
            return type.GetInheritanceChain(self, true).Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);

        if (target.IsInterface)
        {
            if (self && type.IsGenericType && type.GetGenericTypeDefinition() == target)
                return true;

            return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);
        }

        throw new InvalidOperationException($"Type '{target}' cannot be derived from");
    }
}