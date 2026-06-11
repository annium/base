using System.Text.Json.Serialization;
using Annium.Data.Models;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

/// <summary>
/// Tests for materializable JSON converter functionality
/// </summary>
public class MaterializableJsonConverterTest : TestBase
{
    /// <summary>
    /// Tests that deserialization of materializable objects works correctly
    /// </summary>
    [Fact]
    public void Deserialization_Basic_Works()
    {
        // arrange
        var serializer = GetSerializer();

        // act
        var a = serializer.Deserialize<A>(@"{}");

        // assert
        a.Counter.Is(1);
    }

    /// <summary>
    /// Tests that serialization of a materializable object produces correct JSON and does not call OnMaterialized,
    /// and that a subsequent round-trip deserialize increments the counter exactly once
    /// </summary>
    [Fact]
    public void Serialization_Write_ProducesCorrectJsonAndRoundTrips()
    {
        // arrange
        var serializer = GetSerializer();
        var instance = new B { Label = "hello", Score = 7 };

        // act
        var json = serializer.Serialize(instance);

        // assert - Write must not be empty or broken
        json.IsNotNull();
        json.Contains("hello").IsTrue();
        json.Contains("7").IsTrue();

        // act - round-trip: OnMaterialized fires only on Read
        var restored = serializer.Deserialize<B>(json);

        // assert - counter is 0 on the serialized instance (Write did not call OnMaterialized)
        instance.Counter.Is(0);
        // counter is 1 on the deserialized instance (Read called OnMaterialized exactly once)
        restored.Counter.Is(1);
        restored.Label.Is("hello");
        restored.Score.Is(7);
    }

    /// <summary>
    /// Test record implementing IMaterializable interface
    /// </summary>
    internal sealed record A : IMaterializable
    {
        /// <summary>
        /// Gets the counter value incremented during materialization
        /// </summary>
        [JsonIgnore]
        public int Counter { get; private set; }

        /// <summary>
        /// Called when the object is materialized after deserialization
        /// </summary>
        public void OnMaterialized()
        {
            Counter++;
        }
    }

    /// <summary>
    /// Test class with data properties implementing IMaterializable for Write-path testing
    /// </summary>
    internal sealed record B : IMaterializable
    {
        /// <summary>
        /// Gets or sets the label value
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the score value
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets the counter value incremented during materialization
        /// </summary>
        [JsonIgnore]
        public int Counter { get; private set; }

        /// <summary>
        /// Called when the object is materialized after deserialization
        /// </summary>
        public void OnMaterialized()
        {
            Counter++;
        }
    }
}
