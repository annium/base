using System;
using System.Text.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests;

/// <summary>
/// Tests for JsonElementExtensions deserialization helpers
/// </summary>
public class JsonElementExtensionsTest
{
    /// <summary>
    /// Tests that Deserialize&lt;T&gt;(JsonDocument) correctly deserializes a known JSON payload
    /// </summary>
    [Fact]
    public void Deserialize_GenericFromDocument_ReturnsCorrectValue()
    {
        // arrange
        using var doc = JsonDocument.Parse(@"{""X"":3,""Y"":-7}");

        // act
        var result = doc.Deserialize<Point>();

        // assert
        result!.X.Is(3);
        result.Y.Is(-7);
    }

    /// <summary>
    /// Tests that Deserialize(JsonDocument, Type) correctly deserializes a known JSON payload
    /// </summary>
    [Fact]
    public void Deserialize_TypedFromDocument_ReturnsCorrectValue()
    {
        // arrange
        using var doc = JsonDocument.Parse(@"{""X"":5,""Y"":2}");

        // act
        var result = (Point?)doc.Deserialize(typeof(Point));

        // assert
        result!.Value.X.Is(5);
        result.Value.Y.Is(2);
    }

    /// <summary>
    /// Tests that Deserialize(JsonDocument, Type) throws ArgumentNullException for a null document
    /// </summary>
    [Fact]
    public void Deserialize_NullDocument_ThrowsArgumentNullException()
    {
        // arrange
        JsonDocument? doc = null;

        // act / assert
        Wrap.It(() => doc!.Deserialize(typeof(Point))).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that Deserialize&lt;T&gt;(JsonElement) correctly deserializes a JSON element
    /// </summary>
    [Fact]
    public void Deserialize_GenericFromElement_ReturnsCorrectValue()
    {
        // arrange
        using var doc = JsonDocument.Parse(@"{""X"":10,""Y"":-4}");
        var element = doc.RootElement;

        // act
        var result = element.Deserialize<Point>();

        // assert
        result!.X.Is(10);
        result.Y.Is(-4);
    }

    /// <summary>
    /// Tests that Deserialize(JsonElement, Type) correctly deserializes a JSON element
    /// </summary>
    [Fact]
    public void Deserialize_TypedFromElement_ReturnsCorrectValue()
    {
        // arrange
        using var doc = JsonDocument.Parse(@"{""X"":1,""Y"":2}");
        var element = doc.RootElement;

        // act
        var result = (Point?)element.Deserialize(typeof(Point));

        // assert
        result!.Value.X.Is(1);
        result.Value.Y.Is(2);
    }

    /// <summary>
    /// Tests that deserializing a null JSON value via Deserialize&lt;T&gt;(JsonDocument) returns null for a reference type
    /// </summary>
    [Fact]
    public void Deserialize_NullJsonDocument_ReturnsNullForReferenceType()
    {
        // arrange
        using var doc = JsonDocument.Parse("null");

        // act
        var result = doc.Deserialize<string>();

        // assert
        result.Is(null);
    }

    /// <summary>
    /// Test target struct for deserialization
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
