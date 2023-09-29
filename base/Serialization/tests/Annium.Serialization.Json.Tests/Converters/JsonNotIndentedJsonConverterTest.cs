using Annium.Serialization.Json.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

public class JsonNotIndentedJsonConverterTest : TestBase
{
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

    [Fact]
    public void Serialization_Inside_Works()
    {
        // arrange
        var serializer = GetSerializer(opts => opts.WriteIndented = true);

        // act
        var str = serializer.Serialize(new A { Normal = 5, NotIndented = new B { Field = 7 } });

        // assert
        str.Is(@"{
  ""normal"": 5,
  ""notIndented"": {""field"":7}
}");
    }

    internal sealed record A
    {
        public int Normal { get; init; }
        public B NotIndented { get; init; } = default!;
    }

    [JsonNotIndented]
    internal sealed record B
    {
        public int Field { get; init; }
    }
}