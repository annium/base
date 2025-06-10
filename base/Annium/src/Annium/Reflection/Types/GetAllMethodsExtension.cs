using System;
using System.Linq;
using System.Reflection;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for retrieving all methods from a <see cref="Type"/>.
/// </summary>
public static class GetAllMethodsExtension
{
    /// <summary>
    /// Gets all methods of the specified type using the default binding flags.
    /// </summary>
    /// <param name="type">The type to get all methods from.</param>
    /// <returns>An array of <see cref="MethodInfo"/> representing all methods of the type.</returns>
    public static MethodInfo[] GetAllMethods(this Type type) => type.GetAllMethods(Constants.DefaultBindingFlags);

    /// <summary>
    /// Gets all methods of the specified type using the specified binding flags.
    /// </summary>
    /// <param name="type">The type to get all methods from.</param>
    /// <param name="flags">The binding flags to use for retrieving the methods.</param>
    /// <returns>An array of <see cref="MethodInfo"/> representing all methods of the type.</returns>
    public static MethodInfo[] GetAllMethods(this Type type, BindingFlags flags)
    {
        var info = type.GetTypeInfo();

        return info.GetMethods(flags).Concat(info.ImplementedInterfaces.SelectMany(x => x.GetMethods(flags))).ToArray();
    }
}
