using System;
using System.Text.Json;
using Annium.Serialization.Abstractions.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

/// <summary>
/// Tests for enum JSON converter functionality
/// </summary>
public class EnumJsonConverterTest : TestBase
{
    /// <summary>
    /// Tests that serializing a basic enum value produces its string form
    /// </summary>
    [Fact]
    public void Serialization_BasicEnum_ProducesStringForm()
    {
        // arrange
        var serializer = GetSerializer();

        // act
        var result = serializer.Serialize(A.Y);

        // assert
        result.Is(@"""Y""");
    }

    /// <summary>
    /// Tests that serializing a flags enum combination produces the expected flags string
    /// </summary>
    [Fact]
    public void Serialization_FlagsEnum_ProducesFlagsString()
    {
        // arrange
        var serializer = GetSerializer();
        var value = B.Y | B.Z;

        // act
        var result = serializer.Serialize(value);

        // assert
        result.Is(@"""Y, Z""");
    }

    /// <summary>
    /// Tests that basic enum serialization and deserialization works correctly
    /// </summary>
    [Fact]
    public void Serialization_Basic_Works()
    {
        // arrange
        var serializer = GetSerializer();

        // act
        var a1 = serializer.Deserialize<A>(@"""y""");
        var a2 = serializer.Deserialize<A>(@"1");
        var b1 = serializer.Deserialize<B>(@"""Y, z""");
        var b2 = serializer.Deserialize<B>(@"3");
        var c1 = serializer.Deserialize<C>(@"""Y, Z,X""");
        var c2 = serializer.Deserialize<C>(@"""J""");
        var c3 = serializer.Deserialize<C>(@"3");
        var c4 = serializer.Deserialize<C>(@"10");

        // assert
        a1.Is(A.Y);
        a2.Is(A.Y);
        b1.Is(B.Y | B.Z);
        b2.Is(B.Y | B.Z);
        c1.Is(C.X | C.Y | C.Z);
        c2.Is(C.Z);
        c3.Is(C.X | C.Y | C.Z);
        c4.Is(C.Z);
    }

    /// <summary>
    /// Tests that deserializing a boolean JSON token as a regular enum throws JsonException,
    /// because EnumJsonConverter only accepts String or Number tokens.
    /// </summary>
    [Fact]
    public void Deserialization_BasicEnum_BooleanToken_ThrowsJsonException()
    {
        // arrange
        var serializer = GetSerializer();

        // act / assert
        Wrap.It(() => serializer.Deserialize<A>("true")).Throws<JsonException>();
    }

    /// <summary>
    /// Tests that deserializing a boolean JSON token as a flags enum throws JsonException,
    /// because FlagsEnumJsonConverter only accepts String or Number tokens.
    /// </summary>
    [Fact]
    public void Deserialization_FlagsEnum_BooleanToken_ThrowsJsonException()
    {
        // arrange
        var serializer = GetSerializer();

        // act / assert
        Wrap.It(() => serializer.Deserialize<B>("true")).Throws<JsonException>();
    }

    /// <summary>
    /// Test enum A for basic enum testing
    /// </summary>
    internal enum A
    {
        /// <summary>
        /// X value
        /// </summary>
        X,

        /// <summary>
        /// Y value
        /// </summary>
        Y,
    }

    /// <summary>
    /// Test flags enum B for flags enum testing
    /// </summary>
    [Flags]
    internal enum B
    {
        /// <summary>
        /// X flag
        /// </summary>
        X,

        /// <summary>
        /// Y flag
        /// </summary>
        Y,

        /// <summary>
        /// Z flag
        /// </summary>
        Z,
    }

    /// <summary>
    /// Test flags enum C with custom parsing for enum parsing testing
    /// </summary>
    [Flags]
    [EnumParse(",", Z)]
    internal enum C
    {
        /// <summary>
        /// X flag
        /// </summary>
        X,

        /// <summary>
        /// Y flag
        /// </summary>
        Y,

        /// <summary>
        /// Z flag
        /// </summary>
        Z,
    }
}
