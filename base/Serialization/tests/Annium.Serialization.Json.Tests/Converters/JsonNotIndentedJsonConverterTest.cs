using Annium.Serialization.Json.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

/// <summary>
/// Tests for JSON not indented converter functionality
/// </summary>
public class JsonNotIndentedJsonConverterTest : TestBase
{
    /// <summary>
    /// Tests that serialization of objects marked as not indented works correctly
    /// </summary>
    [Fact]
    public void Serialization_Self_Works()
    {
        // arrange
        var serializer = GetSerializer(opts => opts.WriteIndented = true);

        // act
        var str = serializer.Serialize(new B { Field = 7 });

        // assert
        str.Is(@"{""field"":7}");
    }

    /// <summary>
    /// Tests that serialization of not indented objects inside indented objects works correctly
    /// </summary>
    [Fact]
    public void Serialization_Inside_Works()
    {
        // arrange
        var serializer = GetSerializer(opts => opts.WriteIndented = true);

        // act
        var str = serializer.Serialize(
            new A
            {
                Normal = 5,
                NotIndented = new B { Field = 7 },
            }
        );

        // assert
        str.Is(
            @"{
  ""normal"": 5,
  ""notIndented"": {""field"":7}
}"
        );
    }

    /// <summary>
    /// Test record with normal indentation
    /// </summary>
    internal sealed record A
    {
        /// <summary>
        /// Gets the normal property value
        /// </summary>
        public int Normal { get; init; }

        /// <summary>
        /// Gets the not indented property value
        /// </summary>
        public B NotIndented { get; init; } = default!;
    }

    /// <summary>
    /// Test record marked as not indented
    /// </summary>
    [JsonNotIndented]
    internal sealed record B
    {
        /// <summary>
        /// Gets the field value
        /// </summary>
        public int Field { get; init; }
    }
}
