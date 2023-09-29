using System;
using System.Collections.Generic;
using Annium.Collections.Generic;

namespace Annium.Linq;

public static class SortedListGetChunksExtensions
{
    public static IReadOnlyDictionary<(TKey Start, TKey End), ISortedListSpan<TKey, TValue>?> GetChunks<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        TKey start,
        TKey end,
        Func<TKey, TKey> nextKey,
        int chunkSize = 1
    )
        where TKey : notnull
    {
        var compare = source.Comparer.Compare;
        if (compare(start, end) > 0)
            throw new ArgumentException($"Sorted list range {start} - {end} is invalid");

        var result = new Dictionary<(TKey Start, TKey End), ISortedListSpan<TKey, TValue>?>();

        var outChunkStart = start;
        var outChunkEnd = start;
        var inChunkStart = start;
        var prevKey = start;
        var key = start;
        // detect initial state - in or out of chunk
        var index = source.Keys.IndexOf(key);
        var inSource = index >= 0;
        var size = 0;

        while (true)
        {
            // get index of current key
            index = source.Keys.IndexOf(key);

            // go to next key if state is same (in or out)
            if (inSource == index >= 0)
            {
                // break if end
                if (compare(key, end) == 0)
                    break;

                // go to next key
                prevKey = key;
                key = nextKey(key);
                size++;

                continue;
            }

            // state has changed
            inSource = !inSource;

            // if entering chunk
            if (index >= 0)
            {
                outChunkEnd = prevKey;
                inChunkStart = key;
            }

            // if leaving chunk and chunk size is enough - return new chunk
            if (index == -1 && size >= chunkSize)
            {
                if (compare(outChunkEnd, inChunkStart) < 0)
                    result[(outChunkStart, outChunkEnd)] = null;
                result[(inChunkStart, prevKey)] = source.GetRange(inChunkStart, prevKey);
                outChunkStart = key;
                outChunkEnd = key;
            }

            // break if end
            if (compare(key, end) == 0)
                break;

            // go to next key
            prevKey = key;
            key = nextKey(key);
            size = 1;
        }

        if (inSource)
        {
            // if chunk matches full range
            if (compare(inChunkStart, start) == 0)
                result[(inChunkStart, end)] = source.GetRange(inChunkStart, end);

            // if chunk size is enough
            else if (size >= chunkSize)
            {
                if (compare(outChunkEnd, inChunkStart) < 0)
                    result[(outChunkStart, outChunkEnd)] = null;
                result[(inChunkStart, end)] = source.GetRange(inChunkStart, end);
            }
        }

        if (!inSource)
            result[(outChunkStart, end)] = null;

        return result;
    }
}