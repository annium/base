using System;
using System.Reflection;

namespace Annium.Reflection.Members;

/// <summary>
/// Provides extension methods for setting the value of a property or field from a <see cref="MemberInfo"/>.
/// </summary>
public static class SetPropertyOrFieldValueExtension
{
    /// <summary>
    /// Sets the value of the property or field represented by the specified <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="member">The member info representing a property or field.</param>
    /// <param name="target">The target object to set the value on.</param>
    /// <param name="value">The value to set.</param>
    /// <exception cref="InvalidOperationException">Thrown if the member is neither a writable property nor a field.</exception>
    public static void SetPropertyOrFieldValue(this MemberInfo member, object target, object? value)
    {
        if (member is PropertyInfo property)
        {
            var setter = property.GetSetMethod();
            if (setter is null)
                throw new InvalidOperationException($"property {member} is not writable");
            setter.Invoke(target, new[] { value });
        }
        else if (member is FieldInfo field)
            field.SetValue(target, value);
        else
            throw new InvalidOperationException($"{member} is neither readable property nor field");
    }
}
