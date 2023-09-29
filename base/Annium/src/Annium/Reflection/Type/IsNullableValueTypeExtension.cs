using System;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class IsNullableValueTypeExtension
{
    public static bool IsNullableValueType(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.IsValueType && Nullable.GetUnderlyingType(type) is not null;
    }
}