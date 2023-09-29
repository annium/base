using System.Text.Json.Serialization;
using Annium.Data.Models;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests.Converters;

public class MaterializableJsonConverterTest : TestBase
{
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

    internal sealed record A : IMaterializable
    {
        [JsonIgnore]
        public int Counter { get; private set; }

        public void OnMaterialized()
        {
            Counter++;
        }
    }
}