using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for retrieving all methods from a <see cref="Type"/>.
/// </summary>
public static class GetAllMethodsExtension
{
    /// <summary>
    /// Gets all public instance and static methods of the specified type.
    /// </summary>
    /// <param name="type">The type to get all methods from.</param>
    /// <returns>An array of <see cref="MethodInfo"/> representing all methods of the type.</returns>
    public static MethodInfo[] GetAllMethods(this Type type) => type.GetAllMethods(Constants.PublicBindingFlags);

    /// <summary>
    /// Gets all methods of the specified type using the specified binding flags.
    /// </summary>
    /// <param name="type">The type to get all methods from.</param>
    /// <param name="flags">The binding flags to use for retrieving the methods.</param>
    /// <returns>An array of <see cref="MethodInfo"/> representing all methods of the type.</returns>
    public static MethodInfo[] GetAllMethods(this Type type, BindingFlags flags) =>
        MembersIncludingInterfaces.Get(type, flags, (i, f) => i.GetMethods(f));
}
