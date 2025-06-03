using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests for enum mapping functionality in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map enums between different types
/// - Handle enum value conversion
/// - Preserve enum values during mapping
/// - Apply enum mapping rules correctly
/// </remarks>
public class EnumTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    /// <remarks>
    /// Registers the mapper with a profile that defines enum mapping rules.
    /// </remarks>
    public EnumTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile<EnumProfile>());
    }

    /// <summary>
    /// Tests that enum mapping works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Enums can be mapped between different types
    /// - Enum values are correctly mapped
    /// - The mapping preserves the original values
    /// - The result is a valid instance of the target enum
    /// </remarks>
    [Fact]
    public void Map_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new A { Value = SourceEnum.One };

        // act
        var result = mapper.Map<B>(value);

        // assert
        result.Value.Is(TargetEnum.One);
    }

    /// <summary>
    /// Tests that enum mapping with different values works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Enums can be mapped with different values
    /// - Enum value conversion is handled correctly
    /// - The mapping preserves the original values
    /// - The result is a valid instance of the target enum
    /// </remarks>
    [Fact]
    public void Map_WithDifferentValues_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new A { Value = SourceEnum.Two };

        // act
        var result = mapper.Map<B>(value);

        // assert
        result.Value.Is(TargetEnum.Two);
    }

    /// <summary>
    /// Profile that defines enum mapping rules for various types.
    /// </summary>
    /// <remarks>
    /// Configures mapping for:
    /// - A to B with enum mapping
    /// - SourceEnum to TargetEnum value conversion
    /// </remarks>
    private class EnumProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the EnumProfile class.
        /// </summary>
        public EnumProfile()
        {
            Map<A, B>();
            Map<SourceEnum, TargetEnum>();
        }
    }

    /// <summary>
    /// Source enum for testing enum mapping.
    /// </summary>
    private enum SourceEnum
    {
        /// <summary>
        /// First value.
        /// </summary>
        One,

        /// <summary>
        /// Second value.
        /// </summary>
        Two
    }

    /// <summary>
    /// Target enum for testing enum mapping.
    /// </summary>
    private enum TargetEnum
    {
        /// <summary>
        /// First value.
        /// </summary>
        One,

        /// <summary>
        /// Second value.
        /// </summary>
        Two
    }

    /// <summary>
    /// Source class with enum property.
    /// </summary>
    private class A
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public SourceEnum Value { get; set; }
    }

    /// <summary>
    /// Target class with enum property.
    /// </summary>
    private class B
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public TargetEnum Value { get; set; }
    }
} 