using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for retrieving the inheritance chain of a <see cref="Type"/>.
/// </summary>
public static class GetInheritanceChainExtension
{
    /// <summary>
    /// Gets the inheritance chain of the specified type.
    /// </summary>
    /// <param name="type">The type to get the inheritance chain for.</param>
    /// <param name="self">If true, includes the type itself in the chain.</param>
    /// <param name="root">If true, includes the root type (<see cref="object"/> or <see cref="ValueType"/>) in the chain.</param>
    /// <returns>An array of <see cref="Type"/> representing the inheritance chain.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    public static Type[] GetInheritanceChain(this Type type, bool self = false, bool root = false)
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
