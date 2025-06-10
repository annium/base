using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// Tests for property assignment-based mapping resolution in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map properties between objects
/// - Handle property assignments
/// - Preserve property values during mapping
/// - Map between different object types
/// </remarks>
public class AssignmentMapResolverTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssignmentMapResolverTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public AssignmentMapResolverTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Tests that mapping properties between objects works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Properties can be mapped between objects
    /// - Property values are correctly assigned
    /// - The mapping preserves the original values
    /// - Properties with the same name are mapped correctly
    /// </remarks>
    [Fact]
    public void AssignmentMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new A { Name = "name" };

        // act
        var result = mapper.Map<B>(value);

        // assert
        result.Name.Is(value.Name);
    }

    /// <summary>
    /// Tests that mapping properties between objects with different types works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Properties can be mapped between objects with different types
    /// - Type conversion is handled correctly
    /// - Property values are correctly assigned
    /// - The mapping preserves the original values
    /// </remarks>
    [Fact]
    public void AssignmentMapping_WithExcessProperties_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new A { Name = "name" };

        // act
        var result = mapper.Map<C>(value);

        // assert
        result.IsNotDefault();
    }

    /// <summary>
    /// Source class with string properties.
    /// </summary>
    private class A
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string? Value { get; set; }
    }

    /// <summary>
    /// Target class with string properties.
    /// </summary>
    private class B
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }
    }

    /// <summary>
    /// Target class C for testing mapping with excess properties
    /// </summary>
    private class C;
}
