using System;
using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

/// <summary>
/// Tests for generic dictionary JSON converter functionality
/// </summary>
public class GenericDictionaryJsonConverterTest : TestBase
{
    /// <summary>
    /// Tests that basic dictionary serialization works correctly
    /// </summary>
    [Fact]
    public void Serialization_Basic_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var key = Guid.NewGuid();
        var value = Guid.NewGuid();
        var source = new Dictionary<Guid, Guid> { { key, value } };

        // act
        var result = serializer.Serialize(source);

        // assert
        result.Is($@"{{""{key}"":""{value}""}}");
    }

    /// <summary>
    /// Tests that dictionary serialization with object keys works correctly
    /// </summary>
    [Fact]
    public void Serialization_ObjectKey_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var key = new Key { X = 1, Y = 2 };
        var value = new Key { X = 3, Y = 4 };
        var source = new Dictionary<Key, Key> { { key, value } };

        // act
        var result = serializer.Serialize(source);

        // assert
        result.Is(@"{""{\""x\"":1,\""y\"":2}"":{""x"":3,""y"":4}}");
    }

    /// <summary>
    /// Tests that basic dictionary deserialization works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Basic_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var key = Guid.NewGuid();
        var value = Guid.NewGuid();
        var source = new Dictionary<Guid, Guid> { { key, value } };
        var str = serializer.Serialize(source);

        // act
        var result = serializer.Deserialize<IReadOnlyDictionary<Guid, Guid>>(str);

        // assert
        result.At(key).Is(value);
    }

    /// <summary>
    /// Tests that dictionary deserialization with object keys works correctly
    /// </summary>
    [Fact]
    public void Deserialization_ObjectKey_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var key = new Key { X = 1, Y = 2 };
        var value = new Key { X = 3, Y = 4 };
        var source = new Dictionary<Key, Key> { { key, value } };
        var str = serializer.Serialize(source);

        // act
        var result = serializer.Deserialize<IReadOnlyDictionary<Key, Key>>(str);

        // assert
        result.At(key).Is(value);
    }
}

/// <summary>
/// Test key struct for dictionary key testing
/// </summary>
public struct Key
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
