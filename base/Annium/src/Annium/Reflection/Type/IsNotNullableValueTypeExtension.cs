using System;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class IsNotNullableValueTypeExtension
{
    public static bool IsNotNullableValueType(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.IsValueType && Nullable.GetUnderlyingType(type) is null;
    }
}