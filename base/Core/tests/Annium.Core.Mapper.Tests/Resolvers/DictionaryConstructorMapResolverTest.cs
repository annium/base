using System.Collections.Generic;
using System.Text.Json;
using Annium.Data.Models.Extensions.IsShallowEqual;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// Tests for dictionary constructor-based mapping resolution in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map dictionary values to constructor parameters
/// - Handle dictionary key-value assignments to constructor parameters
/// - Preserve dictionary values during mapping
/// - Map between different types using dictionary values and constructors
/// </remarks>
public class DictionaryConstructorMapResolverTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryConstructorMapResolverTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public DictionaryConstructorMapResolverTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(ConfigureProfile));
    }

    /// <summary>
    /// Tests that mapping dictionary values to constructor parameters works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Dictionary values can be mapped to constructor parameters
    /// - Constructor parameters are correctly assigned from dictionary values
    /// - The mapping preserves the original values
    /// - The object is properly instantiated with the mapped values
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
        result.IsShallowEqual(new C(0, 0, "Alex", 20));
    }

    /// <summary>
    /// Configures the mapping profile for dictionary to object mapping.
    /// </summary>
    /// <param name="p">The profile to configure.</param>
    private void ConfigureProfile(Profile p)
    {
        p.Map<Dictionary<string, object>, C>()
            .Ignore(x => new { x.IgnoredA, x.IgnoredB })
            .For(
                x => new { x.Name, x.Age },
                x => JsonSerializer.Deserialize<Info>(x["Serialized"].ToString()!, default(JsonSerializerOptions))
            );
    }

    /// <summary>
    /// Target class with string constructor parameters.
    /// </summary>
    private class C
    {
        /// <summary>
        /// Ignored field.
        /// </summary>
        public int IgnoredA { get; }

        /// <summary>
        /// Ignored field.
        /// </summary>
        public long IgnoredB { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the age.
        /// </summary>
        public int Age { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="C"/> class.
        /// </summary>
        /// <param name="ignoredA">Ignored field A.</param>
        /// <param name="ignoredB">Ignored field B.</param>
        /// <param name="name">The name to assign.</param>
        /// <param name="age">The age to assign.</param>
        public C(int ignoredA, long ignoredB, string name, int age)
        {
            IgnoredA = ignoredA;
            IgnoredB = ignoredB;
            Name = name;
            Age = age;
        }
    }

    /// <summary>
    /// Information class for deserialization testing.
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
