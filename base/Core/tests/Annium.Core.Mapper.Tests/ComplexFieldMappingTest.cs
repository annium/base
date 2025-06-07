using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Data.Models.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests for complex field mapping scenarios in the mapper.
/// </summary>
/// <remarks>
/// This class tests advanced mapping scenarios including:
/// - Mapping with property assignments
/// - Mapping with constructor parameters
/// - Mapping with serialized/deserialized values
/// - Ignoring specific properties during mapping
/// </remarks>
public class ComplexFieldMappingTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ComplexFieldMappingTest class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    /// <remarks>
    /// Registers the mapper with a custom profile that configures complex mapping scenarios.
    /// </remarks>
    public ComplexFieldMappingTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(ConfigureProfile));
    }

    /// <summary>
    /// Tests mapping with property assignments and serialization.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Properties can be mapped through serialization/deserialization
    /// - Ignored properties are not mapped
    /// - Bidirectional mapping works correctly
    /// - Serialized values are preserved
    /// </remarks>
    [Fact]
    public void AssignmentMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var serialized = JsonSerializer.Serialize(new { Name = "Alex", Age = 20 });
        var value = new A { SerializedValue = serialized };

        // act
        var result = mapper.Map<B>(value);
        var restored = mapper.Map<A>(result);

        // assert
        result.IsShallowEqual(
            new B
            {
                IgnoredA = 0,
                IgnoredB = 0,
                Name = "Alex",
                Age = 20,
            }
        );
        restored.IsShallowEqual(new A { SerializedValue = serialized });
    }

    /// <summary>
    /// Tests mapping with constructor parameters and serialization.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Properties can be mapped through constructor parameters
    /// - Serialized values are correctly deserialized into constructor parameters
    /// - Ignored properties are not mapped
    /// - Bidirectional mapping works correctly
    /// </remarks>
    [Fact]
    public void ConstructorMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var serialized = JsonSerializer.Serialize(new { Name = "Alex", Age = 20 });
        var value = new A { SerializedValue = serialized };

        // act
        var result = mapper.Map<C>(value);
        var restored = mapper.Map<A>(result);

        // assert
        result.IsShallowEqual(new C(0, 0, "Alex", 20));
        restored.IsShallowEqual(new A { SerializedValue = serialized });
    }

    /// <summary>
    /// Configures the mapping profile for complex mapping scenarios.
    /// </summary>
    /// <param name="p">The profile to configure.</param>
    /// <remarks>
    /// Sets up mappings between classes A, B, and C with:
    /// - Property ignoring
    /// - Serialization/deserialization of values
    /// - Bidirectional mapping
    /// </remarks>
    private void ConfigureProfile(Profile p)
    {
        p.Map<A, B>()
            .Ignore(x => new { x.IgnoredA, x.IgnoredB })
            .For(
                x => new { x.Name, x.Age },
                x => JsonSerializer.Deserialize<Serialized>(x.SerializedValue, default(JsonSerializerOptions))
            );
        p.Map<B, A>()
            .For(
                x => x.SerializedValue,
                x => JsonSerializer.Serialize(new { x.Name, x.Age }, default(JsonSerializerOptions))
            );
        p.Map<A, C>()
            .Ignore(x => new { x.IgnoredA, x.IgnoredB })
            .For(
                x => new { x.Name, x.Age },
                x => JsonSerializer.Deserialize<Serialized>(x.SerializedValue, default(JsonSerializerOptions))
            );
        p.Map<C, A>()
            .For(
                x => x.SerializedValue,
                x => JsonSerializer.Serialize(new { x.Name, x.Age }, default(JsonSerializerOptions))
            );
    }

    /// <summary>
    /// Source class containing serialized data.
    /// </summary>
    private class A
    {
        /// <summary>
        /// Gets or sets the serialized value containing data to be mapped
        /// </summary>
        public string SerializedValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Target class with properties for assignment mapping.
    /// </summary>
    private class B
    {
        /// <summary>
        /// Gets or sets a value that should be ignored during mapping
        /// </summary>
        public int IgnoredA { get; set; }

        /// <summary>
        /// Gets or sets a value that should be ignored during mapping
        /// </summary>
        public long IgnoredB { get; set; }

        /// <summary>
        /// Gets or sets the name property
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age property
        /// </summary>
        public int Age { get; set; }
    }

    /// <summary>
    /// Target class with constructor parameters for constructor mapping.
    /// </summary>
    private class C
    {
        /// <summary>
        /// Gets a value that should be ignored during mapping
        /// </summary>
        public int IgnoredA { get; }

        /// <summary>
        /// Gets a value that should be ignored during mapping
        /// </summary>
        public long IgnoredB { get; }

        /// <summary>
        /// Gets the name property
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the age property
        /// </summary>
        public int Age { get; }

        public C(int ignoredA, long ignoredB, string name, int age)
        {
            IgnoredA = ignoredA;
            IgnoredB = ignoredB;
            Name = name;
            Age = age;
        }
    }

    /// <summary>
    /// Class representing the serialized data structure.
    /// </summary>
    private class Serialized
    {
        /// <summary>
        /// Gets or sets the name from serialized data
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age from serialized data
        /// </summary>
        public int Age { get; set; }
    }
}
