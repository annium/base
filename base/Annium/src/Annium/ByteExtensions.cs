using System;

namespace Annium;

/// <summary>
/// Provides extension methods for converting byte arrays and memory to hexadecimal string representations.
/// </summary>
public static class ByteExtensions
{
    /// <summary>
    /// Lookup table for hexadecimal characters.
    /// </summary>
    private static readonly char[] _hexLookup = CreateHexLookup();

    /// <summary>
    /// Converts a byte array to its hexadecimal string representation.
    /// </summary>
    /// <param name="value">The byte array to convert.</param>
    /// <returns>A hexadecimal string representation of the byte array.</returns>
    public static string ToHexString(this byte[] value) => ToHexString(value.AsMemory());

    /// <summary>
    /// Converts a <see cref="Memory{T}"/> to its hexadecimal string representation.
    /// </summary>
    /// <param name="value">The memory of bytes to convert.</param>
    /// <returns>A hexadecimal string representation of the memory.</returns>
    public static string ToHexString(this Memory<byte> value) => ToHexString((ReadOnlyMemory<byte>)value);

    /// <summary>
    /// Converts a <see cref="ReadOnlyMemory{T}"/> to its hexadecimal string representation.
    /// </summary>
    /// <param name="value">The read-only memory of bytes to convert.</param>
    /// <returns>A hexadecimal string representation of the read-only memory.</returns>
    public static string ToHexString(this ReadOnlyMemory<byte> value)
    {
        var lookup = _hexLookup;
        var result = new char[value.Length * 2];

        var i = 0;
        foreach (var item in value.Span)
        {
            result[2 * i] = lookup[2 * item];
            result[2 * i + 1] = lookup[2 * item + 1];
            i++;
        }

        return new string(result);
    }

    /// <summary>
    /// Creates a lookup table for hexadecimal characters.
    /// </summary>
    /// <returns>An array of hexadecimal characters for lookup.</returns>
    private static char[] CreateHexLookup()
    {
        var result = new char[512];

        for (var i = 0; i < 256; i++)
        {
            var s = i.ToString("X2");
            result[2 * i] = s[0];
            result[2 * i + 1] = s[1];
        }

        return result;
    }
}
