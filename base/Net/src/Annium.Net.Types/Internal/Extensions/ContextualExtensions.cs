using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Reflection.Types;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Extensions;

/// <summary>
/// Provides extension methods for working with contextual types from the Namotion.Reflection library.
/// </summary>
internal static class ContextualExtensions
{
    /// <summary>
    /// Gets the friendly display name of a contextual type.
    /// </summary>
    /// <param name="type">The contextual type to get the friendly name for.</param>
    /// <returns>A human-readable string representation of the type name.</returns>
    public static string FriendlyName(this ContextualType type) => type.Type.FriendlyName();

    /// <summary>
    /// Gets the pure name of a contextual type without generic type parameters.
    /// </summary>
    /// <param name="type">The contextual type to get the pure name for.</param>
    /// <returns>The type name without generic type parameter information.</returns>
    public static string PureName(this ContextualType type) => type.Type.PureName();

    /// <summary>
    /// Gets the namespace information for a contextual type.
    /// </summary>
    /// <param name="type">The contextual type to get the namespace for.</param>
    /// <returns>The namespace information of the type.</returns>
    public static Namespace GetNamespace(this ContextualType type) => type.Type.GetNamespace();

    /// <summary>
    /// Gets the generic type arguments for a contextual type.
    /// </summary>
    /// <param name="type">The contextual type to get generic arguments for.</param>
    /// <returns>An array of contextual types representing the generic type arguments. Returns the type's generic arguments directly if available, otherwise gets them from the type definition.</returns>
    public static ContextualType[] GetGenericArguments(this ContextualType type)
    {
        return type.Type is { IsGenericType: true, IsGenericTypeDefinition: true }
            ? type.Type.GetGenericArguments().Select(x => x.ToContextualType()).ToArray()
            : type.GenericArguments;
    }

    /// <summary>
    /// Gets the interfaces directly implemented by the contextual type, excluding inherited interfaces.
    /// </summary>
    /// <param name="type">The contextual type to get own interfaces for.</param>
    /// <returns>A read-only collection of contextual types representing the directly implemented interfaces.</returns>
    public static IReadOnlyCollection<ContextualType> GetOwnInterfaces(this ContextualType type) =>
        type.Type.GetOwnInterfaces().Select(x => x.ToContextualType()).ToArray();

    /// <summary>
    /// Gets all interfaces implemented by the contextual type, including inherited interfaces.
    /// </summary>
    /// <param name="type">The contextual type to get all interfaces for.</param>
    /// <returns>A read-only collection of contextual types representing all implemented interfaces.</returns>
    public static IReadOnlyCollection<ContextualType> GetInterfaces(this ContextualType type) =>
        type.Type.GetInterfaces().Select(x => x.ToContextualType()).ToArray();

    /// <summary>
    /// Gets the members (properties and fields) declared directly on the contextual type, excluding inherited members.
    /// </summary>
    /// <param name="type">The contextual type to get own members for.</param>
    /// <returns>A read-only collection of contextual accessor info representing the type's own members, excluding those inherited from base types.</returns>
    public static IReadOnlyCollection<ContextualAccessorInfo> GetOwnMembers(this ContextualType type)
    {
        var members = type.GetMembers();
        if (type.BaseType is null)
            return members;

        var baseMembers = type.BaseType.GetMembers().Select(x => x.Name).ToHashSet();
        var ownMembers = members.WhereNot(x => baseMembers.Contains(x.Name)).ToArray();

        return ownMembers;
    }

    /// <summary>
    /// Gets all members (properties and fields) of the contextual type, including inherited members.
    /// </summary>
    /// <param name="type">The contextual type to get all members for.</param>
    /// <returns>A read-only collection of contextual accessor info representing all type members.</returns>
    public static IReadOnlyCollection<ContextualAccessorInfo> GetMembers(this ContextualType type) =>
        type.GetProperties().Concat(type.GetFields()).Select(x => x.ToContextualAccessor()).ToArray();

    /// <summary>
    /// Gets the public instance properties declared on the contextual type.
    /// </summary>
    /// <param name="type">The contextual type to get properties for.</param>
    /// <returns>A read-only collection of member info representing the type's public instance properties.</returns>
    private static IReadOnlyCollection<MemberInfo> GetProperties(this ContextualType type) =>
        type.Type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

    /// <summary>
    /// Gets the public instance fields declared on the contextual type.
    /// </summary>
    /// <param name="type">The contextual type to get fields for.</param>
    /// <returns>A read-only collection of member info representing the type's public instance fields.</returns>
    private static IReadOnlyCollection<MemberInfo> GetFields(this ContextualType type) =>
        type.Type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
}
