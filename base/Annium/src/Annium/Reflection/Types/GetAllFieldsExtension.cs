using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for retrieving all fields from a <see cref="Type"/>.
/// </summary>
public static class GetAllFieldsExtension
{
    /// <summary>
    /// Gets all public instance and static fields of the specified type.
    /// </summary>
    /// <param name="type">The type to get all fields from.</param>
    /// <returns>An array of <see cref="FieldInfo"/> representing all fields of the type.</returns>
    public static FieldInfo[] GetAllFields(this Type type) => type.GetAllFields(Constants.PublicBindingFlags);

    /// <summary>
    /// Gets all fields of the specified type using the specified binding flags.
    /// </summary>
    /// <param name="type">The type to get all fields from.</param>
    /// <param name="flags">The binding flags to use for retrieving the fields.</param>
    /// <returns>An array of <see cref="FieldInfo"/> representing all fields of the type.</returns>
    public static FieldInfo[] GetAllFields(this Type type, BindingFlags flags) =>
        MembersIncludingInterfaces.Get(type, flags, (i, f) => i.GetFields(f));
}
