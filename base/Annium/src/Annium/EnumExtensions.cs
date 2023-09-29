using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Annium;

public static class EnumExtensions
{
    #region parse

    public static T ParseEnum<T>(this string str)
        where T : struct, Enum
    {
        if (!str.TryParseEnum<T>(out var value))
            throw new ArgumentException($"'{str}' is not a {typeof(T).Name} value");

        return value;
    }

    public static T ParseEnum<T>(this ValueType raw)
        where T : struct, Enum
    {
        if (!raw.TryParseEnum<T>(out var value))
            throw new ArgumentException($"'{raw}' is not a {typeof(T).Name} value");

        return value;
    }

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

    public static T ParseEnum<T>(this string str, T defaultValue)
        where T : struct, Enum
    {
        if (str.TryParseEnum<T>(out var value))
            return value;

        return defaultValue;
    }

    public static T ParseEnum<T>(this ValueType raw, T defaultValue)
        where T : struct, Enum
    {
        if (raw.TryParseEnum<T>(out var value))
            return value;

        return defaultValue;
    }

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

    public static bool TryParseEnum<T>(this string label, out T value)
        where T : struct, Enum
    {
        var map = ParseLabelsCache.GetOrAdd(typeof(T), ParseLabels);

        if (map.TryGetValue(label.ToLowerInvariant(), out var val))
        {
            value = (T)val;
            return true;
        }

        value = default;
        return false;
    }

    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, ValueType>> ParseLabelsCache =
        new();

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

    public static bool TryParseEnum<T>(this ValueType raw, out T value)
        where T : struct, Enum
    {
        var values = ParseValuesCache.GetOrAdd(typeof(T), ParseValues);

        var val = (ValueType)Convert.ChangeType(raw, Enum.GetUnderlyingType(typeof(T)));
        if (values.Contains(val))
        {
            value = (T)val;
            return true;
        }

        value = default;
        return false;
    }

    private static readonly ConcurrentDictionary<Type, HashSet<ValueType>> ParseValuesCache =
        new();

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

    # region helpers

    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    private static T CastValues<T>(IReadOnlyCollection<T> values)
        where T : struct, Enum
    {
        var typeCode = default(T).GetTypeCode();

        return typeCode switch
        {
            TypeCode.Byte   => (T)(ValueType)values.Cast<byte>().Aggregate(0, (a, v) => a | v),
            TypeCode.UInt16 => (T)(ValueType)values.Cast<ushort>().Aggregate(0, (a, v) => a | v),
            TypeCode.Int32  => (T)(ValueType)values.Cast<int>().Aggregate(0, (a, v) => a | v),
            TypeCode.UInt32 => (T)(ValueType)values.Cast<uint>().Aggregate(0U, (a, v) => a | v),
            TypeCode.Int64  => (T)(ValueType)values.Cast<long>().Aggregate(0L, (a, v) => a | v),
            TypeCode.UInt64 => (T)(ValueType)values.Cast<ulong>().Aggregate(0UL, (a, v) => a | v),
            _               => throw new ArgumentException($"'{typeCode}' based Flags Enum is not supported"),
        };
    }

    #endregion
}