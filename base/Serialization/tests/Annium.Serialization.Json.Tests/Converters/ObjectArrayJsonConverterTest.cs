using System.Text.Json.Serialization;
using Annium.Serialization.Json.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

/// <summary>
/// Tests for object array JSON converter functionality
/// </summary>
public class ObjectArrayJsonConverterTest : TestBase
{
    /// <summary>
    /// Tests that basic object array serialization works correctly
    /// </summary>
    [Fact]
    public void Serialization_Base_Works()
    {
        // arrange
        var serializer = GetSerializer();

        var x = new A { Value = 5, IsFinal = 3 };
        x.SetData("demo");

        // act
        var result = serializer.Serialize(x);

        // assert
        result.Is(@"[""demo"",3,5]");
    }

    /// <summary>
    /// Tests that basic object array deserialization works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Base_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new A { Value = 5, IsFinal = 3 };
        x.SetData("demo");
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<A>(str);

        // assert
        result.Is(x);
    }

    /// <summary>
    /// Tests that object array serialization with ordered properties works correctly
    /// </summary>
    [Fact]
    public void Serialization_Ordered_Works()
    {
        // arrange
        var serializer = GetSerializer();

        var x = new B { Value = 5, IsFinal = 3 };
        x.SetData("demo");

        // act
        var result = serializer.Serialize(x);

        // assert
        result.Is(@"[5,""demo"",3]");
    }

    /// <summary>
    /// Tests that object array deserialization with ordered properties works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Ordered_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new B { Value = 5, IsFinal = 3 };
        x.SetData("demo");
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<B>(str);

        // assert
        result.Is(x);
    }

    /// <summary>
    /// Tests that object array serialization with placeholders and ordered properties works correctly
    /// </summary>
    [Fact]
    public void Serialization_PlaceholdersOrdered_Works()
    {
        // arrange
        var serializer = GetSerializer();

        var x = new C();
        x.SetData("demo");

        // act
        var result = serializer.Serialize(x);

        // assert
        result.Is(@"[null,""demo"",null,null]");
    }

    /// <summary>
    /// Tests that object array deserialization with placeholders and ordered properties works correctly
    /// </summary>
    [Fact]
    public void Deserialization_PlaceholdersOrdered_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var x = new C();
        x.SetData("demo");
        var str = serializer.Serialize(x);

        // act
        var result = serializer.Deserialize<C>(str);

        // assert
        result.Is(x);
    }

    /// <summary>
    /// Test record for basic object array serialization
    /// </summary>
    [JsonAsArray]
    public record A
    {
        /// <summary>
        /// Gets or sets the test value
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets the data string
        /// </summary>
        public string Data { get; private set; } = string.Empty;

        /// <summary>
        /// Test field for final flag
        /// </summary>
        public byte IsFinal;

        /// <summary>
        /// Gets a value indicating whether the value is odd (actually checks if even)
        /// </summary>
        public bool IsOdd => Value % 2 == 0;

        /// <summary>
        /// Sets the data string
        /// </summary>
        /// <param name="data">The data to set</param>
        public void SetData(string data) => Data = data;
    }

    /// <summary>
    /// Test record for ordered object array serialization
    /// </summary>
    [JsonAsArray]
    public record B
    {
        /// <summary>
        /// Gets or sets the test value
        /// </summary>
        [JsonPropertyOrder(0)]
        public int Value { get; set; }

        /// <summary>
        /// Gets the data string
        /// </summary>
        [JsonPropertyOrder(1)]
        public string Data { get; private set; } = string.Empty;

        /// <summary>
        /// Test field for final flag
        /// </summary>
        [JsonPropertyOrder(2)]
        public byte IsFinal;

        /// <summary>
        /// Ignored field for testing
        /// </summary>
        public bool Ignored;

        /// <summary>
        /// Gets a value indicating whether the value is odd (actually checks if even)
        /// </summary>
        public bool IsOdd => Value % 2 == 0;

        /// <summary>
        /// Sets the data string
        /// </summary>
        /// <param name="data">The data to set</param>
        public void SetData(string data) => Data = data;
    }

    /// <summary>
    /// Test record for object array serialization with placeholders
    /// </summary>
    [JsonAsArray]
    [JsonArrayPlaceholder(3, null)]
    public record C
    {
        /// <summary>
        /// Gets the data string
        /// </summary>
        [JsonPropertyOrder(1)]
        public string Data { get; private set; } = string.Empty;

        /// <summary>
        /// Ignored field for testing
        /// </summary>
        public bool Ignored;

        /// <summary>
        /// Sets the data string
        /// </summary>
        /// <param name="data">The data to set</param>
        public void SetData(string data) => Data = data;
    }
}
