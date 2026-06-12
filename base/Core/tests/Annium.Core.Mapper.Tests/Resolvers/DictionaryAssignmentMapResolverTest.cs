using System.Collections.Generic;
using System.Text.Json;
using Annium.Data.Models.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// Verifies that <c>DictionaryAssignmentMapResolver</c> throws <see cref="KeyNotFoundException"/>
/// when the source dictionary is missing a key required by a writable target property.
/// </summary>
public class DictionaryAssignmentMapResolverMissingKeyThrowsTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryAssignmentMapResolverMissingKeyThrowsTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public DictionaryAssignmentMapResolverMissingKeyThrowsTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Maps a dictionary that is missing a key required by the target's writable property.
    /// The compiled mapping expression throws <see cref="KeyNotFoundException"/> at runtime.
    /// The target property is typed <c>object?</c> so <c>InstanceOfMapResolver</c> handles
    /// <c>object→object</c>; the KeyNotFoundException fires when the missing key is accessed.
    /// </summary>
    [Fact]
    public void AssignmentMapping_MissingDictKey_ThrowsKeyNotFoundException()
    {
        // arrange
        var mapper = Get<IMapper>();
        // "Value" key is absent — the resolver's BuildDictKeyAccess emits a throw for it
        var source = new Dictionary<string, object> { { "Other", "irrelevant" } };

        // act + assert
        Wrap.It(() => mapper.Map<Target>(source)).Throws<KeyNotFoundException>();
    }

    /// <summary>
    /// Target with a default constructor and a writable <c>object?</c> property whose name
    /// must appear in the source dictionary.  Using <c>object?</c> ensures
    /// <c>InstanceOfMapResolver</c> can handle the <c>object→object</c> element conversion,
    /// so expression build succeeds and the KeyNotFoundException fires at runtime.
    /// </summary>
    private class Target
    {
        /// <summary>Gets or sets the value that must be supplied by the dictionary.</summary>
        public object? Value { get; set; }
    }
}

/// <summary>
/// Tests for dictionary-based property assignment mapping resolution in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map properties from a dictionary to an object
/// - Handle serialized JSON data in dictionary values
/// - Ignore specified properties during mapping
/// - Apply custom property mapping rules
/// </remarks>
public class DictionaryAssignmentMapResolverTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryAssignmentMapResolverTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public DictionaryAssignmentMapResolverTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(ConfigureProfile));
    }

    /// <summary>
    /// Tests that dictionary-based property assignment mapping works with serialized data.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Properties are correctly mapped from dictionary values
    /// - Serialized JSON data is properly deserialized
    /// - Ignored properties are not mapped
    /// - The mapping process preserves data integrity
    /// </remarks>
    [Fact]
    public void ConstructorMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var serialized = JsonSerializer.Serialize(new { Name = "Alex", Age = 20 });
        var value = new Dictionary<string, object> { { "Serialized", serialized } };

        // act
        var result = mapper.Map<C>(value);

        // assert
        result.IsShallowEqual(new C { Name = "Alex", Age = 20 });
    }

    /// <summary>
    /// Configures the mapping profile for dictionary-based property assignment.
    /// </summary>
    /// <param name="p">The profile to configure.</param>
    /// <remarks>
    /// Sets up:
    /// - Mapping from Dictionary to C
    /// - Property ignoring rules
    /// - Custom property mapping for serialized data
    /// </remarks>
    private void ConfigureProfile(Profile p)
    {
        p.Map<Dictionary<string, object>, C>()
            .Ignore(x => new { x.IgnoredA, x.IgnoredB })
            .For(
                x => new { x.Name, x.Age },
                // "Serialized" is seeded as a boxed JSON string by the test below — ToString() is non-null
                x => JsonSerializer.Deserialize<Info>(x["Serialized"].ToString()!, default(JsonSerializerOptions))
            );
    }

    /// <summary>
    /// Target class with properties for testing dictionary-based mapping.
    /// </summary>
    private class C
    {
        /// <summary>
        /// Gets or sets the first ignored property.
        /// </summary>
        public int IgnoredA { get; set; }

        /// <summary>
        /// Gets or sets the second ignored property.
        /// </summary>
        public long IgnoredB { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        public int Age { get; set; }
    }

    /// <summary>
    /// Class representing deserialized information from JSON.
    /// </summary>
    private class Info
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        public int Age { get; set; }
    }
}
