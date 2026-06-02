using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// G23: Verifies that ConstructorMapResolver handles a value-type (struct) source correctly,
/// exercising the <c>if (src.IsValueType)</c> branch in <c>BuildResolvedBlock</c>.
/// </summary>
public class ConstructorMapResolver_StructSource_WorksTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorMapResolver_StructSource_WorksTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ConstructorMapResolver_StructSource_WorksTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Struct source is routed through ConstructorMapResolver (DstCtor has no default ctor).
    /// The value-type branch in BuildResolvedBlock fires — no null-check scaffolding emitted.
    /// The constructor parameter is satisfied from the matching struct property.
    /// </summary>
    [Fact]
    public void ConstructorMapping_StructSource_MapsViaConstructor()
    {
        // arrange
        var mapper = Get<IMapper>();
        var src = new SrcStruct2 { Name = "v" };

        // act
        var result = mapper.Map<DstCtor>(src);

        // assert
        result.Name.Is("v");
    }

    /// <summary>Value-type source with a single readable string property.</summary>
    private struct SrcStruct2
    {
        /// <summary>Gets or sets Name.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Reference-type target with no default constructor; the single constructor parameter
    /// matches the struct source property by name, satisfying ConstructorMapResolver.
    /// </summary>
    private class DstCtor
    {
        /// <summary>Gets the Name.</summary>
        public string Name { get; }

        /// <summary>Initializes a new instance of <see cref="DstCtor"/>.</summary>
        /// <param name="name">The name value from the source struct.</param>
        public DstCtor(string name) => Name = name;
    }
}

/// <summary>
/// Verifies that <c>ConstructorMapResolver</c> throws <see cref="MappingException"/>
/// when the source type is missing a property required by the target's constructor parameter.
/// </summary>
public class ConstructorMapResolver_MissingProperty_ThrowsTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorMapResolver_MissingProperty_ThrowsTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public ConstructorMapResolver_MissingProperty_ThrowsTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Maps a source that lacks a property matching the target's constructor parameter.
    /// The resolver throws <see cref="MappingException"/> at expression-build time (surfaced via Map).
    /// </summary>
    [Fact]
    public void ConstructorMapping_SourceMissingProperty_ThrowsMappingException()
    {
        // arrange
        var mapper = Get<IMapper>();
        var source = new Source();

        // act + assert — ConstructorMapResolver throws MappingException during expression build
        // because Source has no property matching the "RequiredName" constructor parameter on Target.
        Wrap.It(() => mapper.Map<Target>(source)).Throws<MappingException>();
    }

    /// <summary>Source type that carries no property matching the target's constructor parameter name.</summary>
    private class Source
    {
        /// <summary>Gets or sets a value that is unrelated to the target's constructor parameters.</summary>
        public string? Unrelated { get; set; }
    }

    /// <summary>
    /// Target type with no default constructor whose single parameter ("requiredName") does not
    /// match any property on <see cref="Source"/>.
    /// </summary>
    private class Target
    {
        /// <summary>Gets the required name value.</summary>
        public string RequiredName { get; }

        /// <summary>Initializes a new instance of <see cref="Target"/>.</summary>
        /// <param name="requiredName">Value that must come from a source property named RequiredName.</param>
        public Target(string requiredName)
        {
            RequiredName = requiredName;
        }
    }
}

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
