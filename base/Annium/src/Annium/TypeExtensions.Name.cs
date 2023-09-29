using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium;

public static class TypeNameExtensions
{
    private static readonly ConcurrentDictionary<Type, string> TypeNames = new(new Dictionary<Type, string>
        {
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(bool), "bool" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(void), "void" },
        }
    );

    private static readonly ConcurrentDictionary<Type, string> TypePureNames = new(TypeNames);

    public static string PureName(this Type value) => TypePureNames.GetOrAdd(value, BuildPureName);

    public static string FriendlyName(this Type value) => TypeNames.GetOrAdd(value, BuildFriendlyName);

    private static string BuildPureName(Type type)
    {
        if (type.IsGenericParameter || !type.IsGenericType)
            return CleanupFileLocalName(type.Name);

        var name = CleanupGenericName(CleanupFileLocalName(type.Name));

        return name;
    }

    private static string BuildFriendlyName(Type type)
    {
        if (type.IsGenericParameter || !type.IsGenericType)
            return CleanupFileLocalName(type.Name);

        if (Nullable.GetUnderlyingType(type) is { } nullableUnderlyingType)
            return $"{nullableUnderlyingType.FriendlyName()}?";

        var name = CleanupGenericName(CleanupFileLocalName(type.GetGenericTypeDefinition().Name));
        var arguments = type.GetGenericArguments().Select(x => x.FriendlyName()).ToArray();

        return $"{name}<{string.Join(", ", arguments)}>";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CleanupFileLocalName(string x)
    {
        var separatorIndex = x.IndexOf("__", StringComparison.Ordinal);

        return x.Contains('<') ? x[(separatorIndex + 2)..] : x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CleanupGenericName(string x)
    {
        var tickIndex = x.IndexOf('`');

        return tickIndex >= 0 ? x[..tickIndex] : x;
    }
}