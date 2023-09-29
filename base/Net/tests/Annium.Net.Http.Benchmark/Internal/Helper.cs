using System;
using System.Collections.Concurrent;

namespace Annium.Net.Http.Benchmark.Internal;

internal static class Helper
{
    private static readonly ConcurrentDictionary<int, byte[]> Contents = new();
    private static readonly Random Random = new();

    public static byte[] GetContent(int size) => Contents.GetOrAdd(size, GenerateContent);

    private static byte[] GenerateContent(int size)
    {
        var result = new byte[size];
        Random.NextBytes(result);

        return result;
    }
}