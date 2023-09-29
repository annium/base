using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class TryMakeGenericMethodExtension
{
    public static bool TryMakeGenericMethod(this MethodInfo method, out MethodInfo? result, params Type[] typeArguments)
    {
        if (method is null)
            throw new ArgumentNullException(nameof(method));

        try
        {
            result = method.MakeGenericMethod(typeArguments);

            return true;
        }
        catch
        {
            result = null;

            return false;
        }
    }
}