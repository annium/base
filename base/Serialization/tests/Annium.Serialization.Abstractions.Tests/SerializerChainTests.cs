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
