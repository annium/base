using System;
using System.Collections.Concurrent;

namespace Annium.Net.Http.Benchmark.Internal;

internal static class Helper
{
    private static readonly ConcurrentDictionary<int, byte[]> _contents = new();
    private static readonly Random _random = new();

    public static byte[] GetContent(int size) => _contents.GetOrAdd(size, GenerateContent);

    private static byte[] GenerateContent(int size)
    {
        var result = new byte[size];
        _random.NextBytes(result);

        return result;
    }
}
