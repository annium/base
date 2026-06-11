using System;
using System.Text;
using System.Text.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Abstractions.Tests;

/// <summary>
/// Tests for serializer chaining functionality
/// </summary>
public class SerializerChainTests
{
    /// <summary>
    /// Tests that chaining a generic serializer with a precise serializer works correctly
    /// </summary>
    [Fact]
    public void Chain_GenericPreciseSerializer_Works()
    {
        // arrange
        var generic = Serializer.Create(
            (type, value) => JsonSerializer.Serialize(value, type),
            (type, value) => JsonSerializer.Deserialize(value, type)!
        );
        var precise = Serializer.Create<string, byte[]>(
            value => Encoding.UTF8.GetBytes(value),
            value => Encoding.UTF8.GetString(value)
        );
        var data = new Point { X = 1, Y = -1 };

        // act
        var serializer = generic.Chain(precise);
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize<Point>(serialized);

        // assert
        deserialized.Is(data);
    }

    /// <summary>
    /// Tests that chaining two precise serializers works correctly
    /// </summary>
    [Fact]
    public void Chain_PrecisePreciseSerializer_Works()
    {
        // arrange
        var source = Serializer.Create<string, byte[]>(
            value => Encoding.UTF8.GetBytes(value),
            value => Encoding.UTF8.GetString(value)
        );
        var wrapper = Serializer.Create<string, byte[]>(Convert.FromBase64String, Convert.ToBase64String);
        var data = "demo";

        // act
        var serializer = source.Chain(wrapper);
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize(serialized);

        // assert
        deserialized.Is(data);
    }

    /// <summary>
    /// Tests that chaining a generic serializer with a reverse-direction precise wrapper produces correct round-trip
    /// </summary>
    [Fact]
    public void Chain_GenericReverseWrapperSerializer_Works()
    {
        // arrange
        // generic source: ISerializer<string> backed by JSON
        var generic = Serializer.Create(
            (type, value) => JsonSerializer.Serialize(value, type),
            (type, value) => JsonSerializer.Deserialize(value, type)!
        );
        // reverse wrapper: ISerializer<byte[], string>
        //   Serialize(byte[]) → string  (decode UTF8 bytes to string)
        //   Deserialize(string) → byte[] (encode string to UTF8 bytes)
        var reverseWrapper = Serializer.Create<byte[], string>(
            bytes => Encoding.UTF8.GetString(bytes),
            str => Encoding.UTF8.GetBytes(str)
        );
        var data = new Point { X = 7, Y = -3 };

        // act — result is ISerializer<byte[]>
        var serializer = generic.Chain(reverseWrapper);
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize<Point>(serialized);

        // assert
        deserialized.Is(data);
    }

    /// <summary>
    /// Tests that chaining a precise serializer with a reverse-direction precise wrapper produces correct round-trip
    /// </summary>
    [Fact]
    public void Chain_PreciseReverseWrapperSerializer_Works()
    {
        // arrange
        // precise source: ISerializer<string, byte[]>  (UTF8 encode / decode)
        var source = Serializer.Create<string, byte[]>(
            value => Encoding.UTF8.GetBytes(value),
            value => Encoding.UTF8.GetString(value)
        );
        // reverse wrapper: ISerializer<string, byte[]>
        //   Serialize(string) → byte[]  (base64-decode string to bytes)
        //   Deserialize(byte[]) → string (base64-encode bytes to string)
        var reverseWrapper = Serializer.Create<string, byte[]>(
            str => Convert.FromBase64String(str),
            bytes => Convert.ToBase64String(bytes)
        );
        var data = "hello";

        // act — result is ISerializer<string, string>
        var serializer = source.Chain(reverseWrapper);
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize(serialized);

        // assert
        deserialized.Is(data);
    }

    /// <summary>
    /// Test data structure representing a point with X and Y coordinates
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Gets or sets the X coordinate
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate
        /// </summary>
        public int Y { get; set; }
    }
}
