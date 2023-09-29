using Annium.Testing;
using Xunit;

namespace Annium.Serialization.BinaryString.Tests;

public class HexStringSerializerTests : TestBase
{
    [Fact]
    public void Serialization_Deserialization_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var data = new byte[] { 25, 17, 89, 36, 15 };

        // act
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize(serialized);

        // assert
        deserialized.IsEqual(data);
    }
}