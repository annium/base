using System;
using System.Reflection;

namespace Annium.Reflection.Members;

/// <summary>
/// Provides extension methods for retrieving the value of a property or field from a <see cref="MemberInfo"/>.
/// </summary>
public static class GetPropertyOrFieldValueExtension
{
    /// <summary>
    /// Gets the value of the property or field represented by the specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="member">The member info representing a property or field.</param>
    /// <param name="target">The target object to get the value from. If null, the member is assumed to be static.</param>
    /// <returns>The value of the property or field.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the member is neither a readable property nor a field.</exception>
    public static object? GetPropertyOrFieldValue(this MemberInfo member, object? target = null) =>
        member switch
        {
            PropertyInfo property => property.GetMethod?.Invoke(target, Array.Empty<object>()),
            FieldInfo field => field.GetValue(target),
            _ => throw new InvalidOperationException($"{member} is neither readable property nor field"),
        };

    /// <summary>
    /// Gets the value of the property or field represented by the specified <see cref="MemberInfo"/> and casts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="member">The member info representing a property or field.</param>
    /// <param name="target">The target object to get the value from. If null, the member is assumed to be static.</param>
    /// <returns>The value of the property or field cast to the specified type, or default(T) if the value is null.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the member is neither a readable property nor a field.</exception>
    public static T? GetPropertyOrFieldValue<T>(this MemberInfo member, object? target = null)
    {
        var value = member.GetPropertyOrFieldValue(target);

        return value is null ? default : value.CastTo<T>();
    }
}
