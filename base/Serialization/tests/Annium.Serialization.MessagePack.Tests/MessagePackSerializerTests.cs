using Annium.Testing;
using MessagePack;
using Xunit;

namespace Annium.Serialization.MessagePack.Tests;

public class MessagePackSerializerTests : TestBase
{
    [Fact]
    public void Serialization_Deserialization_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var data = new Person { FirstName = "Max", LastName = "Madness" };

        // act
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize<Person>(serialized);

        // assert
        deserialized.IsNotDefault();
        deserialized.FirstName.Is(data.FirstName);
        deserialized.LastName.Is(data.LastName);
    }

    [MessagePackObject]
    public class Person
    {
        [Key(0)]
        public string FirstName { get; set; } = string.Empty;

        [Key(1)]
        public string LastName { get; set; } = string.Empty;
    }
}