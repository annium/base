using System;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class TryMakeGenericTypeExtension
{
    public static bool TryMakeGenericType(this Type type, out Type? result, params Type[] typeArguments)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        try
        {
            result = type.MakeGenericType(typeArguments);

            return true;
        }
        catch
        {
            result = null;

            return false;
        }
    }
}