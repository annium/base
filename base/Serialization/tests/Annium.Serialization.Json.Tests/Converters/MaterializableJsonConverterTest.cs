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
}
