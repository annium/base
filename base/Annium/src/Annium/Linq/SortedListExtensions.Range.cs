using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Collections.Generic;
using Annium.Reflection;

namespace Annium.Linq;

public static class SortedListRangeExtensions
{
    public static void AddRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> range
    )
        where TKey : notnull
    {
        source.InsertRange(range, false);
    }

    public static void SetRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> range
    )
        where TKey : notnull
    {
        source.InsertRange(range, true);
    }

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
            MergeRanges(comparer, targetKeys, targetValues, sourceKeys, sourceValues, ref sourceIndex, rangeKeys, rangeValues, ref rangeIndex, ref targetIndex, replaceDuplicate);

        AppendLeftRangeItems(targetKeys, targetValues, sourceKeys, sourceValues, sourceSize, ref sourceIndex, ref targetIndex);
        AppendLeftRangeItems(targetKeys, targetValues, rangeKeys, rangeValues, rangeValues.Length, ref rangeIndex, ref targetIndex);

        keysField.SetValue(source, targetKeys);
        valuesField.SetValue(source, targetValues);
        sizeField.SetValue(source, targetIndex);
        versionField.SetValue(source, versionField.GetPropertyOrFieldValue<int>(source) + 1);
    }

    private static (TKey[] keys, TValue[] values) InitRangeData<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> range, IComparer<TKey> comparer)
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

    private static void AppendLeftRangeItems<TKey, TValue>(TKey[] targetKeys, TValue[] targetValues, TKey[] sourceKeys, TValue[] sourceValues, int sourceSize, ref int sourceIndex, ref int targetIndex)
        where TKey : notnull
    {
        while (sourceIndex < sourceSize)
            SetItemFromRange(targetKeys, targetValues, sourceKeys, sourceValues, ref sourceIndex, ref targetIndex);
    }

    private static void SetItemFromRange<TKey, TValue>(TKey[] targetKeys, TValue[] targetValues, TKey[] sourceKeys, TValue[] sourceValues, ref int sourceIndex, ref int targetIndex)
        where TKey : notnull
    {
        targetKeys[targetIndex] = sourceKeys[sourceIndex];
        targetValues[targetIndex] = sourceValues[sourceIndex];
        sourceIndex++;
        targetIndex++;
    }

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