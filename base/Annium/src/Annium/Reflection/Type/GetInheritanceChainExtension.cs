using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class GetInheritanceChainExtension
{
    public static Type[] GetInheritanceChain(
        this Type type,
        bool self = false,
        bool root = false
    )
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var chain = new HashSet<Type>();
        if (self)
            chain.Add(type);

        if (type.IsValueType || type == typeof(ValueType))
        {
            if (root)
                chain.Add(typeof(ValueType));

            return chain.ToArray();
        }

        if (type.IsClass)
        {
            if (type.BaseType != null)
                while (type.BaseType != typeof(object))
                {
                    chain.Add(type.BaseType!);
                    type = type.BaseType!;
                }

            if (root)
                chain.Add(typeof(object));

            return chain.ToArray();
        }

        return Array.Empty<Type>();
    }
}