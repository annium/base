using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for retrieving all properties from a <see cref="Type"/>.
/// </summary>
public static class GetAllPropertiesExtension
{
    /// <summary>
    /// Gets all public instance and static properties of the specified type.
    /// </summary>
    /// <param name="type">The type to get all properties from.</param>
    /// <returns>An array of <see cref="PropertyInfo"/> representing all properties of the type.</returns>
    public static PropertyInfo[] GetAllProperties(this Type type) =>
        type.GetAllProperties(Constants.PublicBindingFlags);

    /// <summary>
    /// Gets all properties of the specified type using the specified binding flags.
    /// </summary>
    /// <param name="type">The type to get all properties from.</param>
    /// <param name="flags">The binding flags to use for retrieving the properties.</param>
    /// <returns>An array of <see cref="PropertyInfo"/> representing all properties of the type.</returns>
    public static PropertyInfo[] GetAllProperties(this Type type, BindingFlags flags) =>
        MembersIncludingInterfaces.Get(type, flags, (i, f) => i.GetProperties(f));
}
