using System;
using System.Linq;
using System.Reflection;

namespace Annium.Reflection;

/// <summary>
/// Shared helper for <c>GetAll{Fields,Methods,Properties}</c> extensions — returns the type's
/// declared members of a given kind concatenated with the same kind on every
/// directly-implemented interface.
/// </summary>
internal static class MembersIncludingInterfaces
{
    /// <summary>
    /// Returns members on <paramref name="type"/> plus members of the same kind on all directly-implemented
    /// interfaces.
    /// </summary>
    /// <typeparam name="TMember">The member kind being collected (FieldInfo / MethodInfo / PropertyInfo).</typeparam>
    /// <param name="type">The type to inspect.</param>
    /// <param name="flags">Binding flags forwarded to the per-type accessor.</param>
    /// <param name="getMembers">Accessor that returns members of kind <typeparamref name="TMember"/> from a TypeInfo.</param>
    /// <returns>Concatenated members from the type and its implemented interfaces.</returns>
    public static TMember[] Get<TMember>(
        Type type,
        BindingFlags flags,
        Func<TypeInfo, BindingFlags, TMember[]> getMembers
    )
    {
        var info = type.GetTypeInfo();
        return getMembers(info, flags)
            .Concat(info.ImplementedInterfaces.SelectMany(x => getMembers(x.GetTypeInfo(), flags)))
            .ToArray();
    }
}
