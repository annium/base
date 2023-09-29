using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Reflection;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Extensions;

internal static class ContextualExtensions
{
    public static string FriendlyName(this ContextualType type) => type.Type.FriendlyName();
    public static string PureName(this ContextualType type) => type.Type.PureName();
    public static Namespace GetNamespace(this ContextualType type) => type.Type.GetNamespace();

    public static ContextualType[] GetGenericArguments(this ContextualType type)
    {
        return type.Type is { IsGenericType: true, IsGenericTypeDefinition: true }
            ? type.Type.GetGenericArguments().Select(x => x.ToContextualType()).ToArray()
            : type.GenericArguments;
    }

    public static IReadOnlyCollection<ContextualType> GetOwnInterfaces(this ContextualType type) => type.Type.GetOwnInterfaces()
        .Select(x => x.ToContextualType())
        .ToArray();

    public static IReadOnlyCollection<ContextualType> GetInterfaces(this ContextualType type) => type.Type.GetInterfaces()
        .Select(x => x.ToContextualType())
        .ToArray();

    public static IReadOnlyCollection<ContextualAccessorInfo> GetOwnMembers(this ContextualType type)
    {
        var members = type.GetMembers();
        if (type.BaseType is null)
            return members;

        var baseMembers = type.BaseType.GetMembers().Select(x => x.Name).ToHashSet();
        var ownMembers = members.WhereNot(x => baseMembers.Contains(x.Name)).ToArray();

        return ownMembers;
    }

    public static IReadOnlyCollection<ContextualAccessorInfo> GetMembers(this ContextualType type) => type.GetProperties()
        .Concat(type.GetFields())
        .Select(x => x.ToContextualAccessor())
        .ToArray();

    private static IReadOnlyCollection<MemberInfo> GetProperties(this ContextualType type) => type.Type
        .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

    private static IReadOnlyCollection<MemberInfo> GetFields(this ContextualType type) => type.Type
        .GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
}