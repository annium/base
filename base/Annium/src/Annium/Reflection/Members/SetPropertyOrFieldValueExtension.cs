using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

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
        switch (member)
        {
            case PropertyInfo property:
                var setter =
                    property.GetSetMethod()
                    ?? throw new InvalidOperationException($"property {member} is not writable");
                setter.Invoke(target, [value]);
                break;
            case FieldInfo field:
                field.SetValue(target, value);
                break;
            default:
                throw new InvalidOperationException($"{member} is neither writable property nor field");
        }
    }
}
