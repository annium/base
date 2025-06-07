using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Runtime.Internal.Types;

/// <summary>
/// Internal builder that constructs type hierarchies from collections of types
/// </summary>
internal static class HierarchyBuilder
{
    /// <summary>
    /// Delegate for registering ancestor-descendant relationships
    /// </summary>
    /// <param name="type">The ancestor type</param>
    /// <param name="ancestor">The descendant type</param>
    private delegate void RegisterAncestor(Type type, Type ancestor);

    /// <summary>
    /// Builds a complete type hierarchy from the given collection of types
    /// </summary>
    /// <param name="types">The types to build hierarchy from</param>
    /// <returns>Dictionary mapping ancestors to their descendant collections</returns>
    public static IReadOnlyDictionary<Ancestor, IReadOnlyCollection<Descendant>> BuildHierarchy(
        IReadOnlyCollection<Type> types
    )
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

    /// <summary>
    /// Collects all ancestors for a class type including interfaces and base classes
    /// </summary>
    /// <param name="type">The class type to collect ancestors for</param>
    /// <param name="register">The function to register ancestor relationships</param>
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

    /// <summary>
    /// Collects all ancestors for a struct or interface type
    /// </summary>
    /// <param name="type">The struct or interface type to collect ancestors for</param>
    /// <param name="register">The function to register ancestor relationships</param>
    private static void CollectStructOrInterfaceAncestors(Type type, RegisterAncestor register)
    {
        foreach (var ancestor in type.GetInterfaces())
            register(ancestor, type);
    }
}
