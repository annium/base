using System;
using System.Collections.Concurrent;

namespace Annium.Net.Http.Benchmark.Internal;

/// <summary>
/// Helper class for generating and caching content for HTTP benchmarks.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Cache for generated content by size.
    /// </summary>
    private static readonly ConcurrentDictionary<int, byte[]> _contents = new();

    /// <summary>
    /// Random number generator for content generation.
    /// </summary>
    private static readonly Random _random = new();

    /// <summary>
    /// Gets or generates content of the specified size.
    /// </summary>
    /// <param name="size">The size of content to generate.</param>
    /// <returns>A byte array of the specified size.</returns>
    public static byte[] GetContent(int size) => _contents.GetOrAdd(size, GenerateContent);

    /// <summary>
    /// Generates random content of the specified size.
    /// </summary>
    /// <param name="size">The size of content to generate.</param>
    /// <returns>A byte array filled with random data.</returns>
    private static byte[] GenerateContent(int size)
    {
        var result = new byte[size];
        _random.NextBytes(result);

        return result;
    }
}
