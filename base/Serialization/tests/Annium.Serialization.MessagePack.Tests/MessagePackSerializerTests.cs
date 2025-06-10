using System;
using System.Collections.Generic;
using Annium.Testing;
using MessagePack;
using Xunit;

namespace Annium.Serialization.MessagePack.Tests;

/// <summary>
/// Tests for MessagePack serialization functionality
/// </summary>
public class MessagePackSerializerTests : TestBase
{
    /// <summary>
    /// Tests that MessagePack serialization and deserialization works correctly
    /// </summary>
    [Fact]
    public void Serialization_Deserialization_Works()
    {
        // arrange
        var serializer = GetSerializer();
        var data = new Person
        {
            FirstName = "Max",
            LastName = "Madness",
            Tags = new[] { "a", "b" },
        };

        // act
        var serialized = serializer.Serialize(data);
        var deserialized = serializer.Deserialize<Person>(serialized);

        // assert
        deserialized.IsNotDefault();
        deserialized.FirstName.Is(data.FirstName);
        deserialized.LastName.Is(data.LastName);
    }

    /// <summary>
    /// Test person class for MessagePack serialization testing
    /// </summary>
    [MessagePackObject]
    public class Person
    {
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        [Key(0)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        [Key(1)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tags collection
        /// </summary>
        [Key(2)]
        public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
    }
}
