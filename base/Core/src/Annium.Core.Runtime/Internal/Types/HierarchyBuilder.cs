using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Runtime.Internal.Types;

internal static class HierarchyBuilder
{
    private delegate void RegisterAncestor(Type type, Type ancestor);

    public static IReadOnlyDictionary<Ancestor, IReadOnlyCollection<Descendant>> BuildHierarchy(IReadOnlyCollection<Type> types)
    {
        var result = new Dictionary<Type, HashSet<Type>>();
        var descendantsRegistry = new Dictionary<Type, Descendant>();

        // only classes, structs and interfaces can have/be descendants
        var classes = types.Where(x => x.IsClass).ToArray();
        foreach (var type in classes)
            CollectClassAncestors(type, Register);

        var structsAndInterfaces = types.Where(x => x.IsValueType || x.IsInterface).ToArray();
        foreach (var type in structsAndInterfaces)
            CollectStructOrInterfaceAncestors(type, Register);

        return result.ToDictionary(
            x => new Ancestor(x.Key),
            x => (IReadOnlyCollection<Descendant>)x.Value.Select(xx => descendantsRegistry[xx]).ToArray()
        );

        void Register(Type ancestor, Type descendant)
        {
            if (ancestor.IsGenericType)
                ancestor = ancestor.GetGenericTypeDefinition();

            if (descendant.IsGenericType)
                descendant = descendant.GetGenericTypeDefinition();

            if (result.TryGetValue(ancestor, out var descendants))
                descendants.Add(descendant);
            else
                result[ancestor] = new HashSet<Type> { descendant };

            if (!descendantsRegistry.ContainsKey(descendant))
                descendantsRegistry[descendant] = new Descendant(descendant);
        }
    }

    private static void CollectClassAncestors(Type type, RegisterAncestor register)
    {
        foreach (var ancestor in type.GetInterfaces())
            register(ancestor, type);

        var baseType = type.BaseType;
        while (baseType != null && baseType != typeof(object))
        {
            register(baseType, type);
            baseType = baseType.BaseType;
        }
    }

    private static void CollectStructOrInterfaceAncestors(Type type, RegisterAncestor register)
    {
        foreach (var ancestor in type.GetInterfaces())
            register(ancestor, type);
    }
}