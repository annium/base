using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class GetPropertyOrFieldTypeExtension
{
    public static Type GetPropertyOrFieldType(this MemberInfo member) => member switch
    {
        PropertyInfo property => property.PropertyType,
        FieldInfo field       => field.FieldType,
        _                     => throw new InvalidOperationException($"{member} is neither readable property nor field")
    };
}