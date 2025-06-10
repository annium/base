using System;
using System.Linq;
using System.Reflection;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for retrieving all properties from a <see cref="Type"/>.
/// </summary>
public static class GetAllPropertiesExtension
{
    /// <summary>
    /// Gets all properties of the specified type using the default binding flags.
    /// </summary>
    /// <param name="type">The type to get all properties from.</param>
    /// <returns>An array of <see cref="PropertyInfo"/> representing all properties of the type.</returns>
    public static PropertyInfo[] GetAllProperties(this Type type) =>
        type.GetAllProperties(Constants.DefaultBindingFlags);

    /// <summary>
    /// Gets all properties of the specified type using the specified binding flags.
    /// </summary>
    /// <param name="type">The type to get all properties from.</param>
    /// <param name="flags">The binding flags to use for retrieving the properties.</param>
    /// <returns>An array of <see cref="PropertyInfo"/> representing all properties of the type.</returns>
    public static PropertyInfo[] GetAllProperties(this Type type, BindingFlags flags)
    {
        var info = type.GetTypeInfo();

        return info.GetProperties(flags)
            .Concat(info.ImplementedInterfaces.SelectMany(x => x.GetProperties(flags)))
            .ToArray();
    }
}
