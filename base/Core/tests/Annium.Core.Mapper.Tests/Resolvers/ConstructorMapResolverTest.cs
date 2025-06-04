using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// Tests for constructor-based mapping resolution in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map values to constructor parameters
/// - Handle constructor parameter mapping
/// - Preserve values during mapping
/// - Map between different object types using constructors
/// </remarks>
public class ConstructorMapResolverTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorMapResolverTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public ConstructorMapResolverTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Tests that mapping values to constructor parameters works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Values can be mapped to constructor parameters
    /// - Constructor parameters are correctly assigned
    /// - The mapping preserves the original values
    /// - The object is properly instantiated with the mapped values
    /// </remarks>
    [Fact]
    public void ConstructorMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var first = new A { Name = "first" };
        var second = new A { Name = "second" };

        // act
        var one = mapper.Map<B>(first);
        var arr = mapper.Map<B[]>(new[] { first, second });

        // assert
        one.Name.Is(first.Name);
        arr.Has(2);
        arr.At(0).Name.Is(first.Name);
        arr.At(1).Name.Is(second.Name);
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
    }

    /// <summary>
    /// Target class with constructor parameters.
    /// </summary>
    private class B
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="B"/> class.
        /// </summary>
        /// <param name="name">The name to assign.</param>
        public B(string? name)
        {
            Name = name;
        }
    }
}
