using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Annium;

/// <summary>
/// Provides extension methods for working with enumerations.
/// </summary>
public static class EnumExtensions
{
    #region parse

    /// <summary>
    /// Converts the string representation of an enumeration to its equivalent value.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="str">The string to convert.</param>
    /// <returns>The enumeration value.</returns>
    /// <exception cref="ArgumentException">Thrown when the string is not a valid enumeration value.</exception>
    public static T ParseEnum<T>(this string str)
        where T : struct, Enum
    {
        if (!str.TryParseEnum<T>(out var value))
            throw new ArgumentException($"'{str}' is not a {typeof(T).Name} value");

        return value;
    }

    /// <summary>
    /// Converts the value type representation of an enumeration to its equivalent value.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="raw">The value type to convert.</param>
    /// <returns>The enumeration value.</returns>
    /// <exception cref="ArgumentException">Thrown when the value type is not a valid enumeration value.</exception>
    public static T ParseEnum<T>(this ValueType raw)
        where T : struct, Enum
    {
        if (!raw.TryParseEnum<T>(out var value))
            throw new ArgumentException($"'{raw}' is not a {typeof(T).Name} value");

        return value;
    }

    /// <summary>
    /// Converts the string representation of flags to their equivalent combined value.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="str">The string to convert.</param>
    /// <param name="separator">The separator used to split the string into individual flag values.</param>
    /// <returns>The combined enumeration value.</returns>
    public static T ParseFlags<T>(this string str, string separator)
        where T : struct, Enum
    {
        var values = str.Split(separator)
            .Select(x => x.Trim())
            .Where(x => !x.IsNullOrWhiteSpace())
            .Select(x => x.ParseEnum<T>())
            .ToList();

        if (values.Count == 0)
            return (T)(ValueType)0;

        return CastValues(values);
    }

    #endregion

    #region parse with default

    /// <summary>
    /// Converts the string representation of an enumeration to its equivalent value, or returns a default value if the conversion fails.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="str">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The enumeration value, or the default value if the conversion fails.</returns>
    public static T ParseEnum<T>(this string str, T defaultValue)
        where T : struct, Enum
    {
        if (str.TryParseEnum<T>(out var value))
            return value;

        return defaultValue;
    }

    /// <summary>
    /// Converts the value type representation of an enumeration to its equivalent value, or returns a default value if the conversion fails.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="raw">The value type to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The enumeration value, or the default value if the conversion fails.</returns>
    public static T ParseEnum<T>(this ValueType raw, T defaultValue)
        where T : struct, Enum
    {
        if (raw.TryParseEnum<T>(out var value))
            return value;

        return defaultValue;
    }

    /// <summary>
    /// Converts the string representation of flags to their equivalent combined value, or returns a default value if the conversion fails.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="str">The string to convert.</param>
    /// <param name="separator">The separator used to split the string into individual flag values.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The combined enumeration value, or the default value if the conversion fails.</returns>
    public static T ParseFlags<T>(this string str, string separator, T defaultValue)
        where T : struct, Enum
    {
        var values = str.Split(separator)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.ParseEnum(defaultValue))
            .ToList();

        if (values.Count == 0)
            return defaultValue;

        return CastValues(values);
    }

    #endregion

    #region try parse by label

    /// <summary>
    /// Attempts to convert the string representation of an enumeration to its equivalent value.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="label">The string to convert.</param>
    /// <param name="value">When this method returns, contains the enumeration value if the conversion succeeds, or the default value if the conversion fails.</param>
    /// <returns>true if the conversion succeeds; otherwise, false.</returns>
    public static bool TryParseEnum<T>(this string label, out T value)
        where T : struct, Enum
    {
        var map = _parseLabelsCache.GetOrAdd(typeof(T), ParseLabels);

        if (map.TryGetValue(label.ToLowerInvariant(), out var val))
        {
            value = (T)val;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// A cache of label-to-value mappings for each enumeration type.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, ValueType>> _parseLabelsCache =
        new();

    /// <summary>
    /// Creates a mapping of labels to values for an enumeration type.
    /// </summary>
    /// <param name="type">The enumeration type.</param>
    /// <returns>A dictionary mapping labels to values.</returns>
    private static IReadOnlyDictionary<string, ValueType> ParseLabels(Type type)
    {
        var result = new Dictionary<string, ValueType>();

        var underlyingType = Enum.GetUnderlyingType(type);

        foreach (var item in type.GetFields().Where(x => x.IsStatic))
        {
            var value = (ValueType)item.GetValue(null)!;

            result.Add(item.Name.ToLowerInvariant(), value);
            result.Add(Convert.ChangeType(value, underlyingType).ToString()!, value);

            var descriptionAttribute = item.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
                result.Add(descriptionAttribute.Description.ToLowerInvariant(), value);
        }

        return result;
    }

    #endregion

    #region try parse by value

    /// <summary>
    /// Attempts to convert the value type representation of an enumeration to its equivalent value.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="raw">The value type to convert.</param>
    /// <param name="value">When this method returns, contains the enumeration value if the conversion succeeds, or the default value if the conversion fails.</param>
    /// <returns>true if the conversion succeeds; otherwise, false.</returns>
    public static bool TryParseEnum<T>(this ValueType raw, out T value)
        where T : struct, Enum
    {
        var values = _parseValuesCache.GetOrAdd(typeof(T), ParseValues);

        var val = (ValueType)Convert.ChangeType(raw, Enum.GetUnderlyingType(typeof(T)));
        if (values.Contains(val))
        {
            value = (T)val;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// A cache of valid values for each enumeration type.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, HashSet<ValueType>> _parseValuesCache = new();

    /// <summary>
    /// Creates a set of valid values for an enumeration type.
    /// </summary>
    /// <param name="type">The enumeration type.</param>
    /// <returns>A set of valid values.</returns>
    private static HashSet<ValueType> ParseValues(Type type)
    {
        var valueType = Enum.GetUnderlyingType(type);

        var result = new HashSet<ValueType>();

        // if not flags - simply add all values
        if (type.GetCustomAttribute<FlagsAttribute>() is null)
            foreach (var item in type.GetFields().Where(x => x.IsStatic))
                result.Add((ValueType)Convert.ChangeType(item.GetValue(null)!, valueType));
        else
        {
            var values = type.GetFields()
                .Where(x => x.IsStatic)
                .Select(x => (long)Convert.ChangeType(x.GetValue(null)!, typeof(long)))
                .OrderBy(x => x)
                .ToArray();
            var max = values.Aggregate(0L, static (res, value) => res | value);

            for (var i = values[0]; i <= max; i++)
                result.Add((ValueType)Convert.ChangeType(i, valueType));
        }

        return result;
    }

    #endregion

    #region falgs

    /// <summary>
    /// Enumerate enum values. Returns single value if enum doesn't have flags attribute
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="value">Value to enumerate.</param>
    /// <returns>Enumerable of values, source value contains.</returns>
    public static IReadOnlyCollection<T> EnumerateFlags<T>(this T value)
        where T : struct, Enum
    {
        return Enum.GetValues<T>().Where(x => value.HasFlag(x)).ToArray();
    }

    #endregion

    #region helpers

    /// <summary>
    /// Combines multiple enumeration values into a single value.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="values">The values to combine.</param>
    /// <returns>The combined value.</returns>
    /// <exception cref="ArgumentException">Thrown when the enumeration type is not supported.</exception>
#pragma warning disable CA2021
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    private static T CastValues<T>(IReadOnlyCollection<T> values)
        where T : struct, Enum
    {
        var typeCode = default(T).GetTypeCode();

        return typeCode switch
        {
            TypeCode.Byte => (T)(ValueType)values.Cast<byte>().Aggregate(0, (a, v) => a | v),
            TypeCode.UInt16 => (T)(ValueType)values.Cast<ushort>().Aggregate(0, (a, v) => a | v),
            TypeCode.Int32 => (T)(ValueType)values.Cast<int>().Aggregate(0, (a, v) => a | v),
            TypeCode.UInt32 => (T)(ValueType)values.Cast<uint>().Aggregate(0U, (a, v) => a | v),
            TypeCode.Int64 => (T)(ValueType)values.Cast<long>().Aggregate(0L, (a, v) => a | v),
            TypeCode.UInt64 => (T)(ValueType)values.Cast<ulong>().Aggregate(0UL, (a, v) => a | v),
            _ => throw new ArgumentException($"'{typeCode}' based Flags Enum is not supported"),
        };
    }
#pragma warning restore CA2021

    #endregion
}
