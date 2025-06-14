using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for retrieving the type of a property or field from a <see cref="MemberInfo"/>.
/// </summary>
public static class GetPropertyOrFieldTypeExtension
{
    /// <summary>
    /// Gets the type of the property or field represented by the specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="member">The member info representing a property or field.</param>
    /// <returns>The type of the property or field.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the member is neither a readable property nor a field.</exception>
    public static Type GetPropertyOrFieldType(this MemberInfo member) =>
        member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new InvalidOperationException($"{member} is neither readable property nor field"),
        };
}
