using System;
using System.Text.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Abstractions.Tests;

/// <summary>
/// Tests for serializer factory functionality
/// </summary>
public class SerializerFactoryTests
{
    /// <summary>
    /// Tests that creating a generic serializer works correctly
    /// </summary>
    [Fact]
    public void Create_GenericSerializer_Works()
    {
        // arrange
        Func<Type, object?, string> serialize = (type, value) => JsonSerializer.Serialize(value, type);
        Func<Type, string, object?> deserialize = (type, value) => JsonSerializer.Deserialize(value, type)!;
        var data = new Point { X = 1, Y = -1 };

        // act
        var serializer = Serializer.Create(serialize, deserialize);
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize<Point>(serialized);

        // assert
        serialized.Is(@"{""X"":1,""Y"":-1}");
        deserialized.Is(data);
    }

    /// <summary>
    /// Tests that creating a precise serializer works correctly
    /// </summary>
    [Fact]
    public void Create_PreciseSerializer_Works()
    {
        // arrange
        Func<Point, string> serialize = value => JsonSerializer.Serialize(value);
        Func<string, Point> deserialize = value => JsonSerializer.Deserialize<Point>(value)!;
        var data = new Point { X = 1, Y = -1 };

        // act
        var serializer = Serializer.Create(serialize, deserialize);
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize(serialized);

        // assert
        serialized.Is(@"{""X"":1,""Y"":-1}");
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
