using System;
using System.Text.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Abstractions.Tests;

public class SerializerFactoryTests
{
    [Fact]
    public void Create_GenericSerializer_Works()
    {
        // arrange
        Func<object?, string> serialize = value => JsonSerializer.Serialize(value);
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

    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}