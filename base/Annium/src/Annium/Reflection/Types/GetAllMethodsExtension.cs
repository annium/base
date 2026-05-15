using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for retrieving all methods from a <see cref="Type"/>, including
/// methods declared on every interface the type implements. For the type's own members and its
/// base-class chain only (no interface inclusion), use the BCL <c>Type.GetMethods(BindingFlags)</c>.
/// </summary>
public static class GetAllMethodsExtension
{
    /// <summary>
    /// Gets all public instance and static methods of the specified type AND of every interface it implements.
    /// </summary>
    /// <param name="type">The type to get all methods from.</param>
    /// <returns>An array of <see cref="MethodInfo"/> representing all methods of the type and its implemented interfaces.</returns>
    public static MethodInfo[] GetAllMethods(this Type type) => type.GetAllMethods(Constants.PublicBindingFlags);

    /// <summary>
    /// Gets all methods of the specified type AND of every interface it implements, using the specified binding flags.
    /// </summary>
    /// <param name="type">The type to get all methods from.</param>
    /// <param name="flags">The binding flags to use for retrieving the methods.</param>
    /// <returns>An array of <see cref="MethodInfo"/> representing all methods of the type and its implemented interfaces.</returns>
    public static MethodInfo[] GetAllMethods(this Type type, BindingFlags flags) =>
        MembersIncludingInterfaces.Get(type, flags, (i, f) => i.GetMethods(f));
}
