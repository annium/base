using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Base tests for the mapper functionality.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map objects between different types
/// - Handle property mapping
/// - Preserve values during mapping
/// - Apply mapping rules correctly
/// </remarks>
public class BaseTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    /// <remarks>
    /// Registers the mapper with a profile that defines basic mapping rules.
    /// </remarks>
    public BaseTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Tests that basic object mapping works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Objects can be mapped between different types
    /// - Property values are correctly mapped
    /// - The mapping preserves the original values
    /// - The result is a valid instance of the target type
    /// </remarks>
    [Fact]
    public void SameType_ReturnsSource()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new A { Name = "name" };

        // act
        var result = mapper.Map<A>(value);

        // assert
        result.IsEqual(value);
    }

    /// <summary>
    /// Tests that nested object mapping works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Objects can be mapped between different types
    /// - Property values are correctly mapped
    /// - The mapping preserves the original values
    /// - The result is a valid instance of the target type
    /// </remarks>
    [Fact]
    public void Nesting_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new D(new A { Name = "name" }, "nice");

        // act
        var result = mapper.Map<E>(value);

        // assert
        result.Inner!.Name.Is(value.Inner!.Name);
        result.Value.Is(value.Value);
    }

    /// <summary>
    /// Tests that mapping objects with different property types works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Objects can be mapped with different property types
    /// - Type conversion is handled correctly
    /// - The mapping preserves the original values
    /// - The result is a valid instance of the target type
    /// </remarks>
    [Fact]
    public void NullableNesting_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new D(null, "nice");

        // act
        var result = mapper.Map<E>(value);

        // assert
        result.Inner.IsDefault();
        result.Value.Is(value.Value);
    }

    /// <summary>
    /// Test class A with string properties
    /// </summary>
    private class A
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the value
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
        public string? Name { get; }

        public B(string? name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Test class D with nested structure
    /// </summary>
    private class D
    {
        /// <summary>
        /// Gets the inner A instance
        /// </summary>
        public A? Inner { get; }

        /// <summary>
        /// Gets the value
        /// </summary>
        public string? Value { get; }

        public D(A? inner, string? value)
        {
            Inner = inner;
            Value = value;
        }
    }

    /// <summary>
    /// Test class E with nested structure
    /// </summary>
    private class E
    {
        /// <summary>
        /// Gets or sets the inner B instance
        /// </summary>
        public B? Inner { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string? Value { get; set; }
    }
}
