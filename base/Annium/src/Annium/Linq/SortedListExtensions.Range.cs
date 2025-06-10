using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Collections.Generic;
using Annium.Reflection.Members;

namespace Annium.Linq;

/// <summary>Provides extension methods for working with ranges in sorted lists.</summary>
public static class SortedListRangeExtensions
{
    /// <summary>Adds a range of key-value pairs to the sorted list, throwing an exception if any key already exists.</summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to add the range to.</param>
    /// <param name="range">The range of key-value pairs to add.</param>
    public static void AddRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> range
    )
        where TKey : notnull
    {
        source.InsertRange(range, false);
    }

    /// <summary>Sets a range of key-value pairs in the sorted list, replacing any existing values for duplicate keys.</summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to set the range in.</param>
    /// <param name="range">The range of key-value pairs to set.</param>
    public static void SetRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> range
    )
        where TKey : notnull
    {
        source.InsertRange(range, true);
    }

    /// <summary>Inserts a range of key-value pairs into the sorted list, with an option to replace duplicate keys.</summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to insert the range into.</param>
    /// <param name="range">The range of key-value pairs to insert.</param>
    /// <param name="replaceDuplicate">Whether to replace existing values for duplicate keys.</param>
    private static void InsertRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> range,
        bool replaceDuplicate
    )
        where TKey : notnull
    {
        var type = source.GetType();
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        var keysField = type.GetField("keys", flags)!;
        var valuesField = type.GetField("values", flags)!;
        var sizeField = type.GetField("_size", flags)!;
        var versionField = type.GetField("version", flags)!;
        var capacityProperty = type.GetProperty("Capacity")!;
        var ensureCapacity = type.GetMethod("EnsureCapacity", flags)!;
        var comparer = (IComparer<TKey>)type.GetField("comparer", flags)!.GetValue(source)!;

        // get source data
        var sourceKeys = keysField.GetPropertyOrFieldValue<TKey[]>(source)!;
        var sourceValues = valuesField.GetPropertyOrFieldValue<TValue[]>(source)!;
        var sourceSize = sizeField.GetPropertyOrFieldValue<int>(source);
        var sourceIndex = 0;

        // get range data
        var (rangeKeys, rangeValues) = InitRangeData(range, comparer);
        var rangeIndex = 0;

        // ensure capacity
        var capacity = capacityProperty.GetPropertyOrFieldValue<int>(source);
        var targetSize = sourceSize + range.Count;
        if (capacity < targetSize)
            ensureCapacity.Invoke(source, new object[] { targetSize });
        capacity = capacityProperty.GetPropertyOrFieldValue<int>(source);

        // get target keys/values arrays (force clean arrays, if capacity is enough)
        var targetKeys = keysField.GetPropertyOrFieldValue<TKey[]>(source)!;
        if (targetKeys == sourceKeys)
            targetKeys = new TKey[capacity];
        var targetValues = valuesField.GetPropertyOrFieldValue<TValue[]>(source)!;
        if (targetValues == sourceValues)
            targetValues = new TValue[capacity];
        var targetIndex = 0;

        while (sourceIndex < sourceSize && rangeIndex < range.Count)
            MergeRanges(
                comparer,
                targetKeys,
                targetValues,
                sourceKeys,
                sourceValues,
                ref sourceIndex,
                rangeKeys,
                rangeValues,
                ref rangeIndex,
                ref targetIndex,
                replaceDuplicate
            );

        AppendLeftRangeItems(
            targetKeys,
            targetValues,
            sourceKeys,
            sourceValues,
            sourceSize,
            ref sourceIndex,
            ref targetIndex
        );
        AppendLeftRangeItems(
            targetKeys,
            targetValues,
            rangeKeys,
            rangeValues,
            rangeValues.Length,
            ref rangeIndex,
            ref targetIndex
        );

        keysField.SetValue(source, targetKeys);
        valuesField.SetValue(source, targetValues);
        sizeField.SetValue(source, targetIndex);
        versionField.SetValue(source, versionField.GetPropertyOrFieldValue<int>(source) + 1);
    }

    /// <summary>Initializes arrays of keys and values from a dictionary, sorted by key.</summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="range">The dictionary to extract keys and values from.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <returns>A tuple containing sorted arrays of keys and values.</returns>
    private static (TKey[] keys, TValue[] values) InitRangeData<TKey, TValue>(
        IReadOnlyDictionary<TKey, TValue> range,
        IComparer<TKey> comparer
    )
        where TKey : notnull
    {
        var keys = new TKey[range.Count];
        var values = new TValue[range.Count];
        var index = 0;

        foreach (var (rangeKey, rangeValue) in range.OrderBy(x => x.Key, comparer))
        {
            keys[index] = rangeKey;
            values[index] = rangeValue;
            index++;
        }

        return (keys, values);
    }

    /// <summary>Merges two sorted ranges of key-value pairs into a target array.</summary>
    /// <typeparam name="TKey">The type of the keys in the ranges.</typeparam>
    /// <typeparam name="TValue">The type of the values in the ranges.</typeparam>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="targetKeys">The target array for keys.</param>
    /// <param name="targetValues">The target array for values.</param>
    /// <param name="sourceKeys">The source array for keys.</param>
    /// <param name="sourceValues">The source array for values.</param>
    /// <param name="sourceIndex">The current index in the source array (passed by reference).</param>
    /// <param name="rangeKeys">The range array for keys.</param>
    /// <param name="rangeValues">The range array for values.</param>
    /// <param name="rangeIndex">The current index in the range array (passed by reference).</param>
    /// <param name="targetIndex">The current index in the target array (passed by reference).</param>
    /// <param name="replaceDuplicate">Whether to replace existing values for duplicate keys.</param>
    private static void MergeRanges<TKey, TValue>(
        IComparer<TKey> comparer,
        TKey[] targetKeys,
        TValue[] targetValues,
        TKey[] sourceKeys,
        TValue[] sourceValues,
        ref int sourceIndex,
        TKey[] rangeKeys,
        TValue[] rangeValues,
        ref int rangeIndex,
        ref int targetIndex,
        bool replaceDuplicate
    )
        where TKey : notnull
    {
        switch (comparer.Compare(sourceKeys[sourceIndex], rangeKeys[rangeIndex]))
        {
            case < 0:
                SetItemFromRange(targetKeys, targetValues, sourceKeys, sourceValues, ref sourceIndex, ref targetIndex);
                break;
            case > 0:
                SetItemFromRange(targetKeys, targetValues, rangeKeys, rangeValues, ref rangeIndex, ref targetIndex);
                break;
            default:
                if (!replaceDuplicate)
                    throw new InvalidOperationException($"Trying to add duplicate key {rangeKeys[rangeIndex]}");
                SetItemFromRange(targetKeys, targetValues, rangeKeys, rangeValues, ref rangeIndex, ref targetIndex);
                sourceIndex++;
                break;
        }
    }

    /// <summary>Appends remaining items from a source range to the target arrays.</summary>
    /// <typeparam name="TKey">The type of the keys in the arrays.</typeparam>
    /// <typeparam name="TValue">The type of the values in the arrays.</typeparam>
    /// <param name="targetKeys">The target array for keys.</param>
    /// <param name="targetValues">The target array for values.</param>
    /// <param name="sourceKeys">The source array for keys.</param>
    /// <param name="sourceValues">The source array for values.</param>
    /// <param name="sourceSize">The size of the source array.</param>
    /// <param name="sourceIndex">The current index in the source array (passed by reference).</param>
    /// <param name="targetIndex">The current index in the target array (passed by reference).</param>
    private static void AppendLeftRangeItems<TKey, TValue>(
        TKey[] targetKeys,
        TValue[] targetValues,
        TKey[] sourceKeys,
        TValue[] sourceValues,
        int sourceSize,
        ref int sourceIndex,
        ref int targetIndex
    )
        where TKey : notnull
    {
        while (sourceIndex < sourceSize)
            SetItemFromRange(targetKeys, targetValues, sourceKeys, sourceValues, ref sourceIndex, ref targetIndex);
    }

    /// <summary>Sets a single key-value pair from a source range to the target arrays.</summary>
    /// <typeparam name="TKey">The type of the keys in the arrays.</typeparam>
    /// <typeparam name="TValue">The type of the values in the arrays.</typeparam>
    /// <param name="targetKeys">The target array for keys.</param>
    /// <param name="targetValues">The target array for values.</param>
    /// <param name="sourceKeys">The source array for keys.</param>
    /// <param name="sourceValues">The source array for values.</param>
    /// <param name="sourceIndex">The current index in the source array (passed by reference).</param>
    /// <param name="targetIndex">The current index in the target array (passed by reference).</param>
    private static void SetItemFromRange<TKey, TValue>(
        TKey[] targetKeys,
        TValue[] targetValues,
        TKey[] sourceKeys,
        TValue[] sourceValues,
        ref int sourceIndex,
        ref int targetIndex
    )
        where TKey : notnull
    {
        targetKeys[targetIndex] = sourceKeys[sourceIndex];
        targetValues[targetIndex] = sourceValues[sourceIndex];
        sourceIndex++;
        targetIndex++;
    }

    /// <summary>Gets a span of the sorted list between the specified start and end keys.</summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to get the range from.</param>
    /// <param name="start">The start key of the range.</param>
    /// <param name="end">The end key of the range.</param>
    /// <returns>A span of the sorted list between the specified keys, or null if the keys are not found.</returns>
    public static ISortedListSpan<TKey, TValue>? GetRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        TKey start,
        TKey end
    )
        where TKey : notnull
    {
        var startIndex = source.Keys.IndexOf(start);
        var endIndex = source.Keys.IndexOf(end);

        if (startIndex < 0 || endIndex < 0)
            return null;

        return new SortedListSpan<TKey, TValue>(source, startIndex, endIndex - startIndex + 1);
    }
}
