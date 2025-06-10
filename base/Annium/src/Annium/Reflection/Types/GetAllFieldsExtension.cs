using System;
using System.Linq;
using System.Reflection;

namespace Annium.Reflection.Types;

/// <summary>
/// Provides extension methods for retrieving all fields from a <see cref="Type"/>.
/// </summary>
public static class GetAllFieldsExtension
{
    /// <summary>
    /// Gets all fields of the specified type using the default binding flags.
    /// </summary>
    /// <param name="type">The type to get all fields from.</param>
    /// <returns>An array of <see cref="FieldInfo"/> representing all fields of the type.</returns>
    public static FieldInfo[] GetAllFields(this Type type) => type.GetAllFields(Constants.DefaultBindingFlags);

    /// <summary>
    /// Gets all fields of the specified type using the specified binding flags.
    /// </summary>
    /// <param name="type">The type to get all fields from.</param>
    /// <param name="flags">The binding flags to use for retrieving the fields.</param>
    /// <returns>An array of <see cref="FieldInfo"/> representing all fields of the type.</returns>
    public static FieldInfo[] GetAllFields(this Type type, BindingFlags flags)
    {
        var info = type.GetTypeInfo();

        return info.GetFields(flags).Concat(info.ImplementedInterfaces.SelectMany(x => x.GetFields(flags))).ToArray();
    }
}
