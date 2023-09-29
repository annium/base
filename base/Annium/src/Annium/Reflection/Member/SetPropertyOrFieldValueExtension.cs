using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class SetPropertyOrFieldValueExtension
{
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