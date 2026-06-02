using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// G23: Verifies that AssignmentMapResolver handles a value-type (struct) source correctly,
/// exercising the <c>if (src.IsValueType)</c> branch in <c>BuildResolvedBlock</c>.
/// </summary>
public class AssignmentMapResolver_StructSource_WorksTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssignmentMapResolver_StructSource_WorksTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public AssignmentMapResolver_StructSource_WorksTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Struct source is routed through AssignmentMapResolver (DstClass has a default ctor).
    /// The value-type branch in BuildResolvedBlock fires — no null-check scaffolding emitted.
    /// All readable struct properties must map to the matching writable class properties.
    /// </summary>
    [Fact]
    public void AssignmentMapping_StructSource_MapsAllProperties()
    {
        // arrange
        var mapper = Get<IMapper>();
        var src = new SrcStruct { X = 1, Y = 2 };

        // act
        var result = mapper.Map<DstClass>(src);

        // assert
        result.X.Is(1);
        result.Y.Is(2);
    }

    /// <summary>Value-type source with two readable properties.</summary>
    private struct SrcStruct
    {
        /// <summary>Gets or sets X.</summary>
        public int X { get; set; }

        /// <summary>Gets or sets Y.</summary>
        public int Y { get; set; }
    }

    /// <summary>Reference-type target with a default constructor and matching writable properties.</summary>
    private class DstClass
    {
        /// <summary>Gets or sets X.</summary>
        public int X { get; set; }

        /// <summary>Gets or sets Y.</summary>
        public int Y { get; set; }
    }
}

/// <summary>
/// Verifies that <c>AssignmentMapResolver</c> throws <see cref="MappingException"/>
/// when the target type has a writable property that has no matching property on the source type.
/// </summary>
public class AssignmentMapResolver_MissingSourceProperty_ThrowsTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssignmentMapResolver_MissingSourceProperty_ThrowsTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public AssignmentMapResolver_MissingSourceProperty_ThrowsTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Maps from a source that is missing a property required by the target.
    /// The resolver throws <see cref="MappingException"/> at expression-build time (surfaced via Map).
    /// </summary>
    [Fact]
    public void AssignmentMapping_TargetPropertyAbsentOnSource_ThrowsMappingException()
    {
        // arrange
        var mapper = Get<IMapper>();
        var source = new Source { Name = "test" };

        // act + assert — AssignmentMapResolver throws MappingException during expression build
        // because Source has no property matching the "Extra" writable property on Target.
        Wrap.It(() => mapper.Map<Target>(source)).Throws<MappingException>();
    }

    /// <summary>Source type with a single property; has no "Extra" property.</summary>
    private class Source
    {
        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Target type with a default constructor and a writable property "Extra" absent on <see cref="Source"/>.</summary>
    private class Target
    {
        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }

        /// <summary>Gets or sets an extra value that has no counterpart on <see cref="Source"/>.</summary>
        public int Extra { get; set; }
    }
}

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
        var value = new A { Name = "name", Value = "excess" };

        // act
        var result = mapper.Map<C>(value);

        // assert
        // the matching property maps; the source-only Value has no target on C and is dropped without throwing
        result.Name.Is(value.Name);
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
    /// Target class C for testing mapping with excess properties — carries only the matching Name,
    /// so the source-only Value property must be dropped during mapping.
    /// </summary>
    private class C
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }
    }
}
