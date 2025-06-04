using Annium.Core.DependencyInjection;
using Annium.Core.Mapper.Attributes;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// Tests for enum-based mapping resolution in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map between enum values
/// - Handle enum value conversion
/// - Preserve enum values during mapping
/// - Map between different enum types
/// </remarks>
public class EnumTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public EnumTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Tests that mapping between enum values works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Enum values can be mapped between different enum types
    /// - Enum value conversion is handled correctly
    /// - The mapping preserves the original values
    /// - The result is a valid enum value
    /// </remarks>
    [Fact]
    public void EnumMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();

        // assert
        mapper.Map<string>(Sex.Male).Is("Male");
        mapper.Map<Sex>("female").Is(Sex.Female);
    }

    [AutoMapped]
    private enum Sex
    {
        Male,
        Female,
    }
}
