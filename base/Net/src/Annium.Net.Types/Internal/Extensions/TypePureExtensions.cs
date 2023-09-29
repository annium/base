using System;

namespace Annium.Net.Types.Internal.Extensions;

internal static class TypePureExtensions
{
    public static Type GetPure(this Type type)
    {
        return type.TryGetPure() ?? throw new InvalidOperationException($"Can't resolve pure type of {type}");
    }

    public static Type? TryGetPure(this Type type)
    {
        if (type.IsGenericType)
            return type.GetGenericTypeDefinition();

        return type.IsClass || type.IsValueType || type.IsInterface ? type : null;
    }
}