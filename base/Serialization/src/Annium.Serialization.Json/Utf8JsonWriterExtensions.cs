using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Annium.Serialization.Json;

/// <summary>
/// Extension methods for Utf8JsonWriter providing additional writing functionality.
/// </summary>
public static class Utf8JsonWriterExtensions
{
    /// <summary>
    /// Maximum length for formatting decimal values as quoted strings.
    /// </summary>
    private const int MaximumFormatDecimalLength = 33; // default 31 (i.e. 'G') + 2 bytes for quotes

    /// <summary>
    /// Writes a property name and value pair to the JSON writer.
    /// </summary>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteProperty<T>(
        this Utf8JsonWriter writer,
        ReadOnlySpan<char> propertyName,
        T value,
        JsonSerializerOptions options
    )
    {
        writer.WritePropertyName(propertyName);
        JsonSerializer.Serialize(writer, value, options);
    }

    /// <summary>
    /// Writes a property name and decimal value as a quoted string.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The decimal value to write as a string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteNumberString(this Utf8JsonWriter writer, ReadOnlySpan<char> propertyName, decimal value)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteNumberStringValue(value);
    }

    /// <summary>
    /// Writes a decimal value as a quoted string value.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The decimal value to write as a string.</param>
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
