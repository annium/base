using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium;

/// <summary>
/// Provides extension methods for working with type names.
/// </summary>
[SuppressMessage("Style", "IDE0306:Simplify collection initialization")]
public static class TypeNameExtensions
{
    /// <summary>
    /// A dictionary that maps types to their friendly names.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, string> _typeNames = new(
        new Dictionary<Type, string>
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

    /// <summary>
    /// A dictionary that maps types to their pure names.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, string> _typePureNames = new(_typeNames);

    /// <summary>
    /// Gets the pure name of the type, without generic parameters.
    /// </summary>
    /// <param name="value">The type to get the pure name for.</param>
    /// <returns>The pure name of the type.</returns>
    public static string PureName(this Type value) => _typePureNames.GetOrAdd(value, BuildPureName);

    /// <summary>
    /// Gets the friendly name of the type, including generic parameters.
    /// </summary>
    /// <param name="value">The type to get the friendly name for.</param>
    /// <returns>The friendly name of the type.</returns>
    public static string FriendlyName(this Type value) => _typeNames.GetOrAdd(value, BuildFriendlyName);

    /// <summary>
    /// Builds the pure name of a type.
    /// </summary>
    /// <param name="type">The type to build the pure name for.</param>
    /// <returns>The pure name of the type.</returns>
    private static string BuildPureName(Type type)
    {
        if (type.IsGenericParameter || !type.IsGenericType)
            return CleanupFileLocalName(type.Name);

        var name = CleanupGenericName(CleanupFileLocalName(type.Name));

        return name;
    }

    /// <summary>
    /// Builds the friendly name of a type.
    /// </summary>
    /// <param name="type">The type to build the friendly name for.</param>
    /// <returns>The friendly name of the type.</returns>
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

    /// <summary>
    /// Cleans up a file-local name by removing the file-local prefix.
    /// </summary>
    /// <param name="x">The name to clean up.</param>
    /// <returns>The cleaned up name.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CleanupFileLocalName(string x)
    {
        var separatorIndex = x.IndexOf("__", StringComparison.Ordinal);

        return x.Contains('<') ? x[(separatorIndex + 2)..] : x;
    }

    /// <summary>
    /// Cleans up a generic name by removing the generic parameter count.
    /// </summary>
    /// <param name="x">The name to clean up.</param>
    /// <returns>The cleaned up name.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CleanupGenericName(string x)
    {
        var tickIndex = x.IndexOf('`');

        return tickIndex >= 0 ? x[..tickIndex] : x;
    }
}
