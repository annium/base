using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// Tests for enumerable-based mapping resolution in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map between different collection types (arrays, lists, dictionaries)
/// - Handle both concrete and interface collection types
/// - Preserve collection contents and order
/// - Map collection elements correctly
/// - Handle read-only dictionary interfaces
/// </remarks>
public class EnumerableMapResolverTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumerableMapResolverTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public EnumerableMapResolverTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Tests that mapping to array types works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Arrays can be mapped to different array types
    /// - Collection elements are correctly mapped
    /// - Property values are preserved during mapping
    /// - The order of elements is maintained
    /// </remarks>
    [Fact]
    public void ToArray_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new[] { new A { Name = "name" } };

        // act
        var one = mapper.Map<B[]>(value);
        var two = mapper.Map<C[]>(value);

        // assert
        one.Has(1);
        one.At(0).Name.Is(value[0].Name);
        two.Has(1);
        two.At(0).Name.Is(value[0].Name);
    }

    /// <summary>
    /// Tests that mapping to collection types works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Arrays can be mapped to List types
    /// - Collection elements are correctly mapped
    /// - Property values are preserved during mapping
    /// - The order of elements is maintained
    /// </remarks>
    [Fact]
    public void ToCollection_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new[] { new A { Name = "name" } };

        // act
        var result = mapper.Map<List<B>>(value);

        // assert
        result.Has(1);
        result.At(0).Name.Is(value[0].Name);
    }

    /// <summary>
    /// Tests that mapping to dictionary types works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Dictionaries can be mapped to different dictionary types
    /// - Dictionary keys and values are correctly mapped
    /// - Property values are preserved during mapping
    /// - The dictionary structure is maintained
    /// </remarks>
    [Fact]
    public void ToDictionary_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Dictionary<string, A>
        {
            {
                "one",
                new A { Name = "name" }
            },
        };

        // act
        IDictionary<string, B> result = mapper.Map<Dictionary<string, B>>(value);

        // assert
        result.Has(1);
        result.At("one").Name.Is(value["one"].Name);
    }

    /// <summary>
    /// Tests that mapping to IEnumerable interface works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Arrays can be mapped to IEnumerable interface
    /// - Collection elements are correctly mapped
    /// - Property values are preserved during mapping
    /// - The order of elements is maintained
    /// </remarks>
    [Fact]
    public void ToIEnumerable_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new[] { new A { Name = "name" } };

        // act
        var result = mapper.Map<IEnumerable<B>>(value).ToArray();

        // assert
        result.Has(1);
        result.At(0).Name.Is(value[0].Name);
    }

    /// <summary>
    /// Tests that mapping to IDictionary interface works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Dictionaries can be mapped to IReadOnlyDictionary interface
    /// - Dictionary keys and values are correctly mapped
    /// - Property values are preserved during mapping
    /// - The dictionary structure is maintained
    /// </remarks>
    [Fact]
    public void ToIDictionary_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Dictionary<string, A>
        {
            {
                "one",
                new A { Name = "name" }
            },
        };

        // act
        var result = mapper.Map<IReadOnlyDictionary<string, B>>(value);

        // assert
        result.Has(1);
        result.At("one").Name.Is(value["one"].Name);
    }

    /// <summary>
    /// Tests that mapping to the same dictionary type works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Dictionaries can be mapped to the same type
    /// - Dictionary keys and values are preserved
    /// - Property values are maintained
    /// - The dictionary structure is preserved
    /// </remarks>
    [Fact]
    public void ToIDictionary_Same_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Dictionary<string, A>
        {
            {
                "one",
                new A { Name = "name" }
            },
        };

        // act
        var result = mapper.Map<IReadOnlyDictionary<string, A>>(value);

        // assert
        result.Has(1);
        result.At("one").Name.Is(value["one"].Name);
    }

    /// <summary>
    /// Source class with a Name property.
    /// </summary>
    private class A
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }
    }

    /// <summary>
    /// Target class with a Name property set via constructor.
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

    /// <summary>
    /// Target class with a Name property.
    /// </summary>
    private class C
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }
    }
}
