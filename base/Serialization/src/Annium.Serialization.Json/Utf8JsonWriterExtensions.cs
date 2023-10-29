using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Annium.Serialization.Json;

public static class Utf8JsonWriterExtensions
{
    private const int MaximumFormatDecimalLength = 33; // default 31 (i.e. 'G') + 2 bytes for quotes

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteProperty<T>(this Utf8JsonWriter writer, ReadOnlySpan<char> propertyName, T value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(propertyName);
        JsonSerializer.Serialize(writer, value, options);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteNumberString(this Utf8JsonWriter writer, ReadOnlySpan<char> propertyName, decimal value)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteNumberStringValue(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteNumberStringValue(this Utf8JsonWriter writer, decimal value)
    {
        Span<byte> data = stackalloc byte[MaximumFormatDecimalLength];
        data[0] = (byte)'"';
        Utf8Formatter.TryFormat(value, data[1..], out var bytesWritten);
        data[bytesWritten + 1] = (byte)'"';
        writer.WriteRawValue(data[..(bytesWritten + 2)]);
    }
}