using System;
using System.Text;
using System.Text.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Abstractions.Tests;

public class SerializerChainTests
{
    [Fact]
    public void Chain_GenericPreciseSerializer_Works()
    {
        // arrange
        var generic = Serializer.Create(
            value => JsonSerializer.Serialize(value),
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

    [Fact]
    public void Chain_PrecisePreciseSerializer_Works()
    {
        // arrange
        var source = Serializer.Create<string, byte[]>(
            value => Encoding.UTF8.GetBytes(value),
            value => Encoding.UTF8.GetString(value)
        );
        var wrapper = Serializer.Create<string, byte[]>(
            Convert.FromBase64String,
            Convert.ToBase64String
        );
        var data = "demo";

        // act
        var serializer = source.Chain(wrapper);
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize(serialized);

        // assert
        deserialized.Is(data);
    }

    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}