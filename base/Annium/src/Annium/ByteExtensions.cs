using System;

namespace Annium;

public static class ByteExtensions
{
    private static readonly char[] HexLookup = CreateHexLookup();

    public static string ToHexString(this byte[] value) => ToHexString(value.AsMemory());

    public static string ToHexString(this Memory<byte> value) => ToHexString((ReadOnlyMemory<byte>)value);

    public static string ToHexString(this ReadOnlyMemory<byte> value)
    {
        var lookup = HexLookup;
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

    private static char[] CreateHexLookup()
    {
        var result = new char[512];

        for (int i = 0; i < 256; i++)
        {
            var s = i.ToString("X2");
            result[2 * i] = s[0];
            result[2 * i + 1] = s[1];
        }

        return result;
    }
}