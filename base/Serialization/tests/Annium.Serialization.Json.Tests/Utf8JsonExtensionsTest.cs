using System;
using System.Buffers;
using System.Text.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests;

/// <summary>
/// Tests for Utf8JsonWriterExtensions and Utf8JsonReaderExtensions
/// </summary>
public class Utf8JsonExtensionsTest
{
    /// <summary>
    /// Tests that writing and reading back a normal decimal value round-trips correctly
    /// </summary>
    [Fact]
    public void WriteNumberStringValue_NormalDecimal_RoundTrips()
    {
        // arrange
        var value = 1.23m;

        // act
        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);
        writer.WriteNumberStringValue(value);
        writer.Flush();

        var json = bufferWriter.WrittenSpan;
        var reader = new Utf8JsonReader(json);
        reader.Read();
        var result = reader.GetDecimalFromString();

        // assert
        result.Is(value);
    }

    /// <summary>
    /// Tests that writing and reading back decimal.MaxValue exercises the full 33-byte buffer and round-trips correctly
    /// </summary>
    [Fact]
    public void WriteNumberStringValue_MaxValueDecimal_RoundTrips()
    {
        // arrange
        // decimal.MaxValue = 79228162514264337593543950335 (29 digits), exercises the full buffer capacity
        var value = decimal.MaxValue;

        // act
        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);
        writer.WriteNumberStringValue(value);
        writer.Flush();

        var json = bufferWriter.WrittenSpan;
        var reader = new Utf8JsonReader(json);
        reader.Read();
        var result = reader.GetDecimalFromString();

        // assert
        result.Is(value);
    }

    /// <summary>
    /// Tests that WriteNumberString writes both the property name and the decimal value as a quoted string
    /// </summary>
    [Fact]
    public void WriteNumberString_PropertyAndValue_ProducesQuotedStringProperty()
    {
        // arrange
        var value = 42.5m;

        // act
        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);
        writer.WriteStartObject();
        writer.WriteNumberString("amount", value);
        writer.WriteEndObject();
        writer.Flush();

        var json = System.Text.Encoding.UTF8.GetString(bufferWriter.WrittenSpan);

        // assert - decimal value must be serialized as a quoted string, not a bare number
        json.Is(@"{""amount"":""42.5""}");
    }

    /// <summary>
    /// Tests that GetDecimalFromString throws JsonException when the string token is not a valid decimal.
    /// Note: Utf8JsonReader is a ref struct and cannot be captured in a lambda; a helper method is used instead.
    /// </summary>
    [Fact]
    public void GetDecimalFromString_InvalidDecimalString_ThrowsJsonException()
    {
        // arrange / act / assert
        // Utf8JsonReader is a ref struct — instantiate it inside the lambda-compatible wrapper method
        Wrap.It(ParseNonDecimalFromString).Throws<JsonException>();
    }

    /// <summary>
    /// Helper that creates a reader over a non-decimal string token and calls GetDecimalFromString.
    /// Extracted from the test method because Utf8JsonReader is a ref struct and cannot be captured in a lambda.
    /// </summary>
    private static void ParseNonDecimalFromString()
    {
        var json = "\"abc\""u8.ToArray();
        var reader = new Utf8JsonReader(json);
        reader.Read();
        reader.GetDecimalFromString();
    }
}
